using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SingularExpress.Models;
using SingularExpress.Models.Models;

namespace SingularExpress.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserTestController : ControllerBase
    {
        private readonly ModelDbContext _context;
        private readonly ILogger<UserTestController> _logger;

        public UserTestController(ModelDbContext context, ILogger<UserTestController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpPost("create-test-user")]
        public async Task<ActionResult<User>> CreateTestUser([FromBody] CreateUserRequest request)
        {
            try
            {
                // Check if user already exists
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower());

                if (existingUser != null)
                {
                    return BadRequest("User with this email already exists.");
                }

                var user = new User
                {
                    UserId = Guid.NewGuid(),
                    UserName = request.UserName,
                    Email = request.Email,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    PasswordHash = AuthController.HashPassword(request.Password),
                    CreatedOn = DateTime.UtcNow,
                    FailedLoginAttempts = 0,
                    IsLockedOut = false
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Remove sensitive data before returning
                user.PasswordHash = string.Empty;

                _logger.LogInformation("Created test user: {Email}", request.Email);

                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating test user: {Email}", request.Email);
                return StatusCode(500, "An error occurred while creating the test user.");
            }
        }

        [HttpGet("all-users")]
        public async Task<ActionResult<List<object>>> GetAllUsers()
        {
            try
            {
                var users = await _context.Users
                    .Select(u => new
                    {
                        u.UserId,
                        u.UserName,
                        u.Email,
                        u.FirstName,
                        u.LastName,
                        u.CreatedOn,
                        u.FailedLoginAttempts,
                        u.IsLockedOut,
                        u.LockoutEnd,
                        u.LastLoginAttempt
                    })
                    .ToListAsync();

                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving users");
                return StatusCode(500, "An error occurred while retrieving users.");
            }
        }

        [HttpDelete("delete-user/{email}")]
        public async Task<ActionResult> DeleteUser(string email)
        {
            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());

                if (user == null)
                {
                    return NotFound("User not found.");
                }

                _context.Users.Remove(user);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Deleted test user: {Email}", email);

                return Ok("User deleted successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user: {Email}", email);
                return StatusCode(500, "An error occurred while deleting the user.");
            }
        }
    }

    public class CreateUserRequest
    {
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
    }
}