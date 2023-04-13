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

    public async Task<ListDataPagination<User>> GetListUsers(UserContext contextUser,
        string searchString,
        int page,
        int size)
    {
        var query = Context.User
            .Where(x => x.IsDeleted == false);

        if (!string.IsNullOrEmpty(searchString))
        {
            searchString = searchString.ToLower().Trim();
            query = query.Where(q => q.Name.ToLower().Contains(searchString) ||
            q.Email.ToLower().Contains(searchString));
        }

        var data = new ListDataPagination<User>
        {
            Page = page,
            TotalItems = await query.CountAsync()
        };
        data.TotalPages = (int)Math.Ceiling((double)data.TotalItems / size);

        data.Data = await query.Skip(size * page)
            .Take(size)
            .AsNoTracking()
            .ToListAsync();

        return data;
    }

    public async Task<User> GetUserByIdAsync(UserContext contextUser, int id)
    {
        var query = await Context.User
            .Where(x => x.IsDeleted == false)
            .SingleOrDefaultAsync(x => x.Id == id);
        return query;
    }

    public async Task<User> GetUserByUsernameAsync(string username)
    {
        var query = await Context.User
            .Where(x => x.IsDeleted == false)
            .SingleOrDefaultAsync(x => x.Username == username);
        return query;
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