using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SingularExpress.Models.Models
{
    [Table("PasswordResetTokens")]
    public class PasswordResetTokens
    {
        [Key]
        [Required]
        [StringLength(255)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        public string Otp { get; set; } = string.Empty;

        [Required]
        public DateTime ExpiresAt { get; set; }
    }
}