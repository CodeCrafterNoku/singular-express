using SingularExpress.Models.Models;

namespace SingularExpress.Interfaces
{
    public interface IUserRepository
    {
        bool CreateUser(User user);
        bool UpdateUser(User user);
        bool DeleteUser(User user);

        User? GetUser(Guid id);
        User? GetUserByUserName(string userName);
        User? GetUserByEmail(string email);
        ICollection<User> GetUsers();

        bool UserExists(Guid userId);
        bool EmailExists(string email, Guid userId);

        bool Save();
    }
}
