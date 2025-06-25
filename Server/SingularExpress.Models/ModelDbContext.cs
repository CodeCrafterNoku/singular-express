using Microsoft.EntityFrameworkCore;
using SingularExpress.Models.Models;

namespace SingularExpress.Models
{
  public class ModelDbContext : DbContext
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="ModelDbContext"/> class.
    /// </summary>
    /// <param name="options">DbContext Options</param>
    public ModelDbContext(DbContextOptions<ModelDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Gets or sets the Users
    /// </summary>
    public DbSet<User> Users { get; set; }
  }
}