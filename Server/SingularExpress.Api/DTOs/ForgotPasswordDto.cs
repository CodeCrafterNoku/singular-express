using System.ComponentModel.DataAnnotations;

namespace SingularExpress.Dto
{
        public class ForgotPasswordDto
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; } = string.Empty;
        }

}