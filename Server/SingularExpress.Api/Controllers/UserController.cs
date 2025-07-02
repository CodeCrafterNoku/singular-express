using Microsoft.AspNetCore.Mvc;
using SingularExpress.Interfaces;
using SingularExpress.Models.Models;
using SingularExpress.Dto;
using Microsoft.AspNetCore.Identity;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authentication; // For sign-in functionality
using System.Security.Claims; // For claims identity
using SingularExpress.Models;






namespace SingularExpress.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly PasswordHasher<User> _passwordHasher;
        private readonly ILogger<UserController> _logger;
        private readonly ModelDbContext _context;
        private readonly IEmailService _emailService;

        // Updated constructor with new dependencies injected
        public UserController(
            IUserRepository userRepository,
            ILogger<UserController> logger,
            ModelDbContext context,
            IEmailService emailService)
        {
            _userRepository = userRepository;
            _passwordHasher = new PasswordHasher<User>();
            _logger = logger;
            _context = context;
            _emailService = emailService;
        }

        // public UserController(IUserRepository userRepository, ILogger<UserController> logger)
        // {
        //     _userRepository = userRepository;
        //     _passwordHasher = new PasswordHasher<User>();
        //     _logger = logger;
        // }

        private bool IsValidPassword(string password)
        {
            string pattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{8,}$";
            return Regex.IsMatch(password, pattern);
        }

        private bool IsValidEmail(string email)
        {
            return !string.IsNullOrWhiteSpace(email) &&
                   email.EndsWith("@singular.co.za", StringComparison.OrdinalIgnoreCase) &&
                   email.IndexOf('@') > 0 &&
                   email.Substring(0, email.IndexOf('@')).Any(char.IsLetter);
        }

        /// <summary>Get all users.</summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<UserDto>), 200)]
        public IActionResult GetUsers()
        {
            var users = _userRepository.GetUsers();

            var dtos = users.Select(u => new UserDto
            {
                UserId = u.UserId,
                UserName = u.UserName,
                Email = u.Email,
                Password = string.Empty,
                FirstName = u.FirstName,
                LastName = u.LastName,
                CreatedOn = u.CreatedOn,
                ModifiedOn = u.ModifiedOn
            }).ToList();

            return Ok(dtos);
        }

        /// <summary>Get user by ID.</summary>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(UserDto), 200)]
        [ProducesResponseType(404)]
        public IActionResult GetUser(Guid id)
        {
            var user = _userRepository.GetUser(id);
            if (user == null) return NotFound();

            var dto = new UserDto
            {
                UserId = user.UserId,
                UserName = user.UserName,
                Email = user.Email,
                Password = string.Empty,
                FirstName = user.FirstName,
                LastName = user.LastName,
                CreatedOn = user.CreatedOn,
                ModifiedOn = user.ModifiedOn
            };

            return Ok(dto);
        }

        /// <summary>Create a new user.</summary>
        [HttpPost]
        [ProducesResponseType(typeof(UserDto), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(409)]
        [ProducesResponseType(500)]
        public IActionResult CreateUser([FromBody] UserDto userDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!IsValidEmail(userDto.Email))
                return BadRequest("Email must be a valid '@singular.co.za' address.");

            if (!IsValidPassword(userDto.Password))
                return BadRequest("Password does not meet complexity requirements.");

            if (_userRepository.GetUsers()
                .Any(u => u.Email.Equals(userDto.Email, StringComparison.OrdinalIgnoreCase)))
                return Conflict("Email is already registered.");

            var user = new User
            {
                UserId = Guid.NewGuid(),
                UserName = userDto.UserName,
                Email = userDto.Email,
                FirstName = userDto.FirstName,
                LastName = userDto.LastName,
                CreatedOn = DateTime.UtcNow
            };

            user.PasswordHash = _passwordHasher.HashPassword(user, userDto.Password);

            var created = _userRepository.CreateUser(user);
            if (!created)
            {
                _logger.LogError("An error occurred while saving the user.");
                return StatusCode(500, "An error occurred while saving the user.");
            }

            userDto.UserId = user.UserId;
            userDto.CreatedOn = user.CreatedOn;
            userDto.ModifiedOn = user.ModifiedOn;
            userDto.Password = string.Empty;

            return CreatedAtAction(nameof(GetUser), new { id = user.UserId }, userDto);
        }

        /// <summary>Update an existing user.</summary>
        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(UserDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(409)]
        [ProducesResponseType(500)]
        public IActionResult UpdateUser(Guid id, [FromBody] UserDto userDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (id != userDto.UserId)
                return BadRequest("User ID mismatch.");

            var existingUser = _userRepository.GetUser(id);
            if (existingUser == null) return NotFound();

            if (!IsValidEmail(userDto.Email))
                return BadRequest("Email must be a valid '@singular.co.za' address.");

            if (!string.IsNullOrEmpty(userDto.Password) && !IsValidPassword(userDto.Password))
                return BadRequest("Password does not meet complexity requirements.");

            if (_userRepository.GetUsers()
                .Any(u => u.Email.Equals(userDto.Email, StringComparison.OrdinalIgnoreCase) && u.UserId != id))
                return Conflict("Email is already registered by another user.");

            existingUser.UserName = userDto.UserName;
            existingUser.Email = userDto.Email;
            existingUser.FirstName = userDto.FirstName;
            existingUser.LastName = userDto.LastName;
            existingUser.ModifiedOn = DateTime.UtcNow;

            if (!string.IsNullOrEmpty(userDto.Password))
            {
                existingUser.PasswordHash = _passwordHasher.HashPassword(existingUser, userDto.Password);
            }

            var updated = _userRepository.UpdateUser(existingUser);
            if (!updated)
            {
                _logger.LogError("An error occurred while updating the user.");
                return StatusCode(500, "An error occurred while updating the user.");
            }

            userDto.Password = string.Empty;
            userDto.CreatedOn = existingUser.CreatedOn;
            userDto.ModifiedOn = existingUser.ModifiedOn;

            return Ok(userDto);
        }

        /// <summary>Delete user by ID.</summary>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public IActionResult DeleteUser(Guid id)
        {
            var user = _userRepository.GetUser(id);
            if (user == null) return NotFound();

            var deleted = _userRepository.DeleteUser(user);
            if (!deleted)
            {
                _logger.LogError("An error occurred while deleting the user.");
                return StatusCode(500, "An error occurred while deleting the user.");
            }

            return NoContent();
        }

        [HttpPost("login")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        public IActionResult Login([FromBody] LoginDto loginDto)
        {
            var user = _userRepository.GetUserByEmail(loginDto.Email);
            if (user == null)
            {
                _logger.LogWarning("Login failed: user not found.");
                return Unauthorized("Invalid email or password.");
            }

            // ✅ Check if account is locked
            if (user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTime.UtcNow)
            {
                _logger.LogWarning("Login failed: account is locked.");
                return Unauthorized($"Account is locked. Try again after {user.LockoutEnd.Value}.");
            }

            var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, loginDto.Password);

            if (result == PasswordVerificationResult.Failed)
            {
                user.FailedLoginAttempts++;

                if (user.FailedLoginAttempts >= 3)
                {
                    user.LockoutEnd = DateTime.UtcNow.AddMinutes(30); // Configurable lockout duration
                    _userRepository.UpdateUser(user); // Make sure this persists the change
                    _logger.LogWarning("Account locked due to multiple failed login attempts.");
                    return Unauthorized("Account locked due to multiple failed login attempts.");
                }

                _userRepository.UpdateUser(user); // Save incremented attempts
                _logger.LogWarning("Login failed: password mismatch.");
                return Unauthorized("Invalid email or password.");
            }

            // ✅ Reset failed attempts on successful login
            user.FailedLoginAttempts = 0;
            user.LockoutEnd = null;
            _userRepository.UpdateUser(user); // Save reset state

            // TODO: Generate and return JWT token if needed
            return Ok("Login successful.");
        }
            
            [HttpPost("forgot-password")]
            [ProducesResponseType(200)]
            [ProducesResponseType(500)]
            public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
            {
                try
                {
                    var user = _userRepository.GetUserByEmail(dto.Email);
                    if (user == null)
                    {
                        // Security: Don't reveal if user doesn't exist
                        return Ok("If an account with this email exists, a reset code has been sent.");
                    }

                    var otp = new Random().Next(100000, 999999).ToString();
                    
                    _context.PasswordResetTokens.Add(new PasswordResetToken
                    {
                        Email = dto.Email,
                        Otp = otp,
                        ExpiresAt = DateTime.UtcNow.AddMinutes(30)
                    });
                    await _context.SaveChangesAsync();

                    await _emailService.SendPasswordResetEmailAsync(
                        dto.Email,
                        otp,
                        $"{user.FirstName} {user.LastName}"
                    );

                    return Ok("If an account with this email exists, a reset code has been sent.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in ForgotPassword");
                    return StatusCode(500, "An error occurred while processing your request.");
                }
            }




    }
}
