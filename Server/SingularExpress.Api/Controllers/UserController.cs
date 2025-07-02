using Microsoft.AspNetCore.Mvc;
using SingularExpress.Interfaces;
using SingularExpress.Models.Models;
using SingularExpress.Dto;
using Microsoft.AspNetCore.Identity;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authentication; // For sign-in functionality
using System.Security.Claims; // For claims identity
using SingularExpress.Api.Services;

namespace SingularExpress.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly PasswordHasher<User> _passwordHasher;
        private readonly ILogger<UserController> _logger;
        private readonly UserLockoutService _lockoutService;

        public UserController(IUserRepository userRepository, ILogger<UserController> logger, UserLockoutService lockoutService)
        {
            _userRepository = userRepository;
            _passwordHasher = new PasswordHasher<User>();
            _logger = logger;
            _lockoutService = lockoutService;
        }

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
        [ProducesResponseType(423)] // Locked account
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            // Check if user is currently locked out
            var isLockedOut = await _lockoutService.IsUserLockedOutAsync(loginDto.Email);
            if (isLockedOut)
            {
                var remainingTime = await _lockoutService.GetRemainingLockoutTimeAsync(loginDto.Email);
                _logger.LogWarning("Login attempt blocked: user {Email} is locked out.", loginDto.Email);
                return StatusCode(423, new { 
                    message = "Account is temporarily locked due to multiple failed login attempts.", 
                    remainingLockoutTime = remainingTime?.ToString(@"mm\:ss")
                });
            }

            var user = _userRepository.GetUserByEmail(loginDto.Email);
            if (user == null)
            {
                _logger.LogWarning("Login failed: user not found.");
                return Unauthorized("Invalid email or password.");
            }

            var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, loginDto.Password);
            if (result == PasswordVerificationResult.Failed)
            {
                // Record failed login attempt
                await _lockoutService.RecordFailedLoginAttemptAsync(loginDto.Email);
                _logger.LogWarning("Login failed: password mismatch for user {Email}. Failed attempts recorded.", loginDto.Email);
                
                // Check if user is now locked out
                isLockedOut = await _lockoutService.IsUserLockedOutAsync(loginDto.Email);
                if (isLockedOut)
                {
                    var remainingTime = await _lockoutService.GetRemainingLockoutTimeAsync(loginDto.Email);
                    return StatusCode(423, new { 
                        message = "Account has been locked due to multiple failed login attempts.", 
                        remainingLockoutTime = remainingTime?.ToString(@"mm\:ss")
                    });
                }
                
                return Unauthorized("Invalid email or password.");
            }

            // Successful login - reset failed attempts
            await _lockoutService.ResetFailedLoginAttemptsAsync(loginDto.Email);
            _logger.LogInformation("User {Email} logged in successfully.", loginDto.Email);

            // You can later generate a JWT token here if needed
            return Ok(new { message = "Login successful.", userId = user.UserId });
        }



    }
}
