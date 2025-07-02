using SingularExpress.Models;
using SingularExpress.Models.Models;
using SingularExpress.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace SingularExpress.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly ModelDbContext _context;

        public UserRepository(ModelDbContext context)
        {
            _context = context;
        }

        public bool CreateUser(User user)
        {
            _context.Users.Add(user);
            return Save();
        }

        public bool UpdateUser(User user)
        {
            _context.Users.Update(user);
            return Save();
        }

        public bool DeleteUser(User user)
        {
            _context.Users.Remove(user);
            return Save();
        }

        public User? GetUser(Guid id)
        {
            return _context.Users.FirstOrDefault(u => u.UserId == id);
        }

        public User? GetUserByUserName(string userName)
        {
            return _context.Users.FirstOrDefault(u => u.UserName == userName);
        }

        public User? GetUserByEmail(string email)
        {
            return _context.Users.FirstOrDefault(u => u.Email == email);
        }

        public ICollection<User> GetUsers()
        {
            return _context.Users.OrderBy(u => u.UserName).ToList();
        }

        public bool UserExists(Guid userId)
        {
            return _context.Users.Any(u => u.UserId == userId);
        }

        public bool EmailExists(string email, Guid userId)
        {
            return _context.Users.Any(u => u.Email.ToLower() == email.ToLower() && u.UserId != userId);
        }

        public bool Save()
        {
            return _context.SaveChanges() > 0;
        }
    }
}
