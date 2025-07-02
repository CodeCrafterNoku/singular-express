using SingularExpress.Models;
using SingularExpress.Models.Models;
using Microsoft.EntityFrameworkCore;

namespace SingularExpress.Api.Services
{
    public class UserLockoutService
    {
        private readonly ModelDbContext _context;
        private const int MaxFailedAttempts = 3;
        private const int LockoutDurationMinutes = 15;

        public UserLockoutService(ModelDbContext context)
        {
            _context = context;
        }

        public async Task<bool> IsUserLockedOutAsync(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            
            if (user == null) return false;

            return user.LockoutEnd.HasValue && user.LockoutEnd > DateTime.UtcNow;
        }

        public async Task RecordFailedLoginAttemptAsync(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            
            if (user == null) return;

            user.FailedLoginAttempts++;
            
            if (user.FailedLoginAttempts >= MaxFailedAttempts)
            {
                user.LockoutEnd = DateTime.UtcNow.AddMinutes(LockoutDurationMinutes);
            }

            await _context.SaveChangesAsync();
        }

        public async Task ResetFailedLoginAttemptsAsync(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            
            if (user == null) return;

            user.FailedLoginAttempts = 0;
            user.LockoutEnd = null;

            await _context.SaveChangesAsync();
        }

        public async Task<TimeSpan?> GetRemainingLockoutTimeAsync(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            
            if (user?.LockoutEnd == null || user.LockoutEnd <= DateTime.UtcNow)
                return null;

            return user.LockoutEnd.Value - DateTime.UtcNow;
        }
    }
}