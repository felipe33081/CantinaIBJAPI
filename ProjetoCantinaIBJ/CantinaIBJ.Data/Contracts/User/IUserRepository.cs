using CantinaIBJ.Data.Contracts.Core;
using CantinaIBJ.Model;
using CantinaIBJ.Model.User;

namespace CantinaIBJ.Data.Contracts;

public interface IUserRepository : IRepositoryBase<User>
{
    Task<IEnumerable<User>> GetUsers(UserContext contextUser);
    Task<User> GetUserByIdAsync(UserContext contextUser, int id);
    Task<User> GetUserByUsernameAsync(string username);
    Task AddUserAsync(UserContext contextUser, User user);
    Task UpdateUserAsync(UserContext contextUser, User user);

    bool VerifyPassword(User user, string password);
}