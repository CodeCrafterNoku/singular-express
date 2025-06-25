using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace SingularExpress.Models
{
    public class ModelDbContextFactory : IDesignTimeDbContextFactory<ModelDbContext>
    {
        public ModelDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ModelDbContext>();

            // Adjust the path and connection string as needed for your environment
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var connectionString = configuration.GetConnectionString("DefaultConnection");
            optionsBuilder.UseSqlServer(connectionString);

            return new ModelDbContext(optionsBuilder.Options);
        }
    }
}
