using CantinaIBJ.Data.Context;
using CantinaIBJ.Data.Contracts;
using CantinaIBJ.Data.Repositories.Core;
using CantinaIBJ.Model.User;
using CantinaIBJ.Model;
using Microsoft.EntityFrameworkCore;
using System.Text;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.Extensions.Configuration;

namespace CantinaIBJ.Data.Repositories;

public class UserRepository : RepositoryBase<User>, IUserRepository
{
    private readonly IConfiguration _configuration;
    public UserRepository(PostgreSqlContext context, IConfiguration configuration) : base(context)
    {
        _configuration = configuration;
    }

    public async Task<List<User>> GetUsers(UserContext contextUser)
    {
        return await Context.User.ToListAsync();
    }

    public async Task<User> GetUserByIdAsync(UserContext contextUser, int id)
    {
        return await Context.User
            .SingleOrDefaultAsync(x => x.Id == id);
    }

    public async Task<User> GetUserByUsernameAsync(string username)
    {
        return await Context.User
            .SingleOrDefaultAsync(x => x.Username == username);
    }

    public async Task AddUserAsync(UserContext contextUser, User user)
    {
        var passwordHash = HashPassword(user.PasswordHash);

        user.PasswordHash = passwordHash;
        user.CreatedBy = contextUser.GetCurrentUser();

        await Context.AddAsync(user);
        await Context.SaveChangesAsync();
    }

    public async Task UpdateUserAsync(UserContext contextUser, User user)
    {
        var passwordHash = HashPassword(user.PasswordHash);

        user.PasswordHash = passwordHash;
        user.CreatedBy = contextUser.GetCurrentUser();
        user.UpdatedBy = contextUser.GetCurrentUser();

        await Context.SaveChangesAsync();
    }

    private string HashPassword(string password)
    {
        var salt = Encoding.UTF8.GetBytes(_configuration["HashPassword:Key"]);
        var hash = KeyDerivation.Pbkdf2(
            password: password,
            salt: salt,
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: 10000,
            numBytesRequested: 256 / 8
        ); 
        
        return Convert.ToBase64String(hash);
    }

    public bool VerifyPassword(User user, string password)
    {
        var passwordHash = HashPassword(password);

        return user.PasswordHash == passwordHash;
    }
}