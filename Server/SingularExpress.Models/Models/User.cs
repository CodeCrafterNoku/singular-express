using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SingularExpress.Models.Models
{
  [Table("Users")]
  public class User
  {
    public Guid UserId { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessage = "User name is required")]
    [StringLength(50)]
    public string UserName { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false, ErrorMessage = "Email is required")]
    [StringLength(50)]
    public string Email { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false, ErrorMessage = "Password hash is required")]
    public string PasswordHash { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false, ErrorMessage = "First Name is required")]
    [StringLength(50)]
    public string FirstName { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false, ErrorMessage = "Last Name is required")]
    [StringLength(50)]
    public string LastName { get; set; } = string.Empty;

    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

    public DateTime? ModifiedOn { get; set; }

    public int FailedLoginAttempts { get; set; } = 0;
    public DateTime? LockoutEnd { get; set; }


  }
}
