using System;
using System.ComponentModel.DataAnnotations;

namespace SingularExpress.Dto
{
    public class UserDto
    {
        public Guid UserId { get; set; }

        [Required(ErrorMessage = "User name is required")]
        [StringLength(50)]
        public required string UserName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress]
        [StringLength(50)]
        public required string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public required string Password { get; set; }

        [Required(ErrorMessage = "First name is required")]
        [StringLength(50)]
        public required string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(50)]
        public required string LastName { get; set; }

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        public DateTime? ModifiedOn { get; set; }
    }
}