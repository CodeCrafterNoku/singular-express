using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SingularExpress.Models;
using SingularExpress.Models.Models;
using System.Security.Cryptography;
using System.Text;

namespace SingularExpress.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ModelDbContext _context;
        private readonly ILogger<AuthController> _logger;
        
        // Constants for locking mechanism
        private const int MaxFailedAttempts = 3;
        private const int LockoutDurationMinutes = 30; // 30 minutes lockout

        public AuthController(ModelDbContext context, ILogger<AuthController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
        {
            try
            {
                // Find user by email
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower());

                if (user == null)
                {
                    _logger.LogWarning("Login attempt with non-existent email: {Email}", request.Email);
                    return Ok(new LoginResponse
                    {
                        IsSuccess = false,
                        Message = "Invalid email or password.",
                        RemainingAttempts = MaxFailedAttempts
                    });
                }

                // Check if account is currently locked
                if (IsAccountLocked(user))
                {
                    _logger.LogWarning("Login attempt on locked account: {Email}", request.Email);
                    return Ok(new LoginResponse
                    {
                        IsSuccess = false,
                        Message = $"Account is locked due to too many failed attempts. Try again after {user.LockoutEnd:yyyy-MM-dd HH:mm:ss} UTC.",
                        IsLockedOut = true,
                        LockoutEnd = user.LockoutEnd
                    });
                }

                // Verify password
                bool isPasswordValid = VerifyPassword(request.Password, user.PasswordHash);

                // Update last login attempt time
                user.LastLoginAttempt = DateTime.UtcNow;

                if (isPasswordValid)
                {
                    // Successful login - reset failed attempts
                    await ResetFailedAttempts(user);
                    
                    _logger.LogInformation("Successful login for user: {Email}", request.Email);
                    
                    // Remove sensitive data before returning
                    user.PasswordHash = string.Empty;
                    
                    return Ok(new LoginResponse
                    {
                        IsSuccess = true,
                        Message = "Login successful.",
                        User = user,
                        RemainingAttempts = MaxFailedAttempts
                    });
                }
                else
                {
                    // Failed login - increment failed attempts
                    await HandleFailedLogin(user);
                    
                    int remainingAttempts = MaxFailedAttempts - user.FailedLoginAttempts;
                    
                    _logger.LogWarning("Failed login attempt for user: {Email}. Attempts: {Attempts}/{Max}", 
                        request.Email, user.FailedLoginAttempts, MaxFailedAttempts);

                    var response = new LoginResponse
                    {
                        IsSuccess = false,
                        RemainingAttempts = Math.Max(0, remainingAttempts)
                    };

                    if (user.IsLockedOut)
                    {
                        response.Message = $"Account has been locked due to {MaxFailedAttempts} failed attempts. Try again after {user.LockoutEnd:yyyy-MM-dd HH:mm:ss} UTC.";
                        response.IsLockedOut = true;
                        response.LockoutEnd = user.LockoutEnd;
                    }
                    else
                    {
                        response.Message = $"Invalid email or password. {remainingAttempts} attempts remaining.";
                    }

                    return Ok(response);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login attempt for email: {Email}", request.Email);
                return StatusCode(500, new LoginResponse
                {
                    IsSuccess = false,
                    Message = "An error occurred during login. Please try again later."
                });
            }
        }

        [HttpPost("unlock-account")]
        public async Task<ActionResult> UnlockAccount([FromBody] string email)
        {
            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());

                if (user == null)
                {
                    return NotFound("User not found.");
                }

                await ResetFailedAttempts(user);
                
                _logger.LogInformation("Account manually unlocked for user: {Email}", email);
                
                return Ok("Account has been unlocked successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unlocking account for email: {Email}", email);
                return StatusCode(500, "An error occurred while unlocking the account.");
            }
        }

        [HttpGet("account-status/{email}")]
        public async Task<ActionResult<object>> GetAccountStatus(string email)
        {
            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());

                if (user == null)
                {
                    return NotFound("User not found.");
                }

                return Ok(new
                {
                    Email = user.Email,
                    IsLockedOut = user.IsLockedOut,
                    FailedAttempts = user.FailedLoginAttempts,
                    MaxAttempts = MaxFailedAttempts,
                    RemainingAttempts = Math.Max(0, MaxFailedAttempts - user.FailedLoginAttempts),
                    LockoutEnd = user.LockoutEnd,
                    LastLoginAttempt = user.LastLoginAttempt
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting account status for email: {Email}", email);
                return StatusCode(500, "An error occurred while retrieving account status.");
            }
        }

        #region Private Methods

        private bool IsAccountLocked(User user)
        {
            if (!user.IsLockedOut)
                return false;

            // Check if lockout period has expired
            if (user.LockoutEnd.HasValue && DateTime.UtcNow >= user.LockoutEnd.Value)
            {
                // Lockout period expired, auto-unlock the account
                _ = Task.Run(async () => await ResetFailedAttempts(user));
                return false;
            }

            return true;
        }

        private async Task HandleFailedLogin(User user)
        {
            user.FailedLoginAttempts++;
            user.ModifiedOn = DateTime.UtcNow;

            if (user.FailedLoginAttempts >= MaxFailedAttempts)
            {
                user.IsLockedOut = true;
                user.LockoutEnd = DateTime.UtcNow.AddMinutes(LockoutDurationMinutes);
                
                _logger.LogWarning("User account locked due to {Attempts} failed attempts: {Email}", 
                    MaxFailedAttempts, user.Email);
            }

            await _context.SaveChangesAsync();
        }

        private async Task ResetFailedAttempts(User user)
        {
            user.FailedLoginAttempts = 0;
            user.IsLockedOut = false;
            user.LockoutEnd = null;
            user.ModifiedOn = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
        }

        private bool VerifyPassword(string password, string passwordHash)
        {
            // This is a simple implementation. In production, use a proper password hashing library like BCrypt
            using var sha256 = SHA256.Create();
            var hashedPassword = Convert.ToBase64String(sha256.ComputeHash(Encoding.UTF8.GetBytes(password)));
            return hashedPassword == passwordHash;
        }

        // Helper method to hash passwords (for testing or user registration)
        public static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            return Convert.ToBase64String(sha256.ComputeHash(Encoding.UTF8.GetBytes(password)));
        }

        #endregion
    }
}