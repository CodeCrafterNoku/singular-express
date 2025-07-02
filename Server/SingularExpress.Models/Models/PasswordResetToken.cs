using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SingularExpress.Models.Models
{
    [Table("PasswordResetTokens")]  // optional: explicitly name the table
    public class PasswordResetToken
    {
        [Key]
        public int Id { get; set; }  // Primary key for the table

        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MaxLength(10)]
        public string Otp { get; set; } = string.Empty;  // One-time password or token

        [Required]
        public DateTime ExpiresAt { get; set; }  // Expiration time (e.g., UTC now + 1 hour)
    }
}
