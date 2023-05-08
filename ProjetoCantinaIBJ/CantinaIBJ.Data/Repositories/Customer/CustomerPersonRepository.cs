using CantinaIBJ.Data.Context;
using CantinaIBJ.Data.Contracts.Customer;
using CantinaIBJ.Data.Repositories.Core;
using CantinaIBJ.Model;
using CantinaIBJ.Model.Customer;
using Microsoft.EntityFrameworkCore;

namespace CantinaIBJ.Data.Repositories.Customer;

public class CustomerPersonRepository : RepositoryBase<CustomerPerson>, ICustomerPersonRepository
{
    public CustomerPersonRepository(PostgreSqlContext context) : base(context)
    {

    }

    public async Task<int> GetCountList()
    {
        int totalCount;
        return totalCount = Context.CustomerPerson
            .Where(x => x.IsDeleted == false)
            .Count();
    }

    public async Task<ListDataPagination<CustomerPerson>> GetListCustomerPersons(UserContext contextUser, int page, int size, string? name, string? email, string? searchString, bool isDeleted, string? orderBy)
    {
        var query = Context.CustomerPerson
            .Where(x => x.IsDeleted == false);

        if (!string.IsNullOrEmpty(searchString))
        {
            searchString = searchString.ToLower().Trim();
            query = query.Where(q => q.Name.ToLower().Contains(searchString) ||
            q.Email.ToLower().Contains(searchString) ||
            q.Phone.ToLower().Contains(searchString));
        }

        if (!string.IsNullOrEmpty(name))
        {
            query = query.Where(q => q.Name.ToLower().Contains(name));
        }

        if (!string.IsNullOrEmpty(email))
        {
            query = query.Where(q => q.Email.ToLower().Contains(email));
        }

        if (isDeleted)
            query = query.Where(q => q.IsDeleted == true);

        if (!string.IsNullOrEmpty(orderBy))
        {
            switch (orderBy)
            {
                case "id_DESC":
                    query = query.OrderByDescending(t => t.Id);
                    break;
                case "id_ASC":
                    query = query.OrderBy(t => t.Id);
                    break;
                case "name_DESC":
                    query = query.OrderByDescending(t => t.Name);
                    break;
                case "name_ASC":
                    query = query.OrderBy(t => t.Name);
                    break;
                case "balance_DESC":
                    query = query.OrderByDescending(t => t.Balance);
                    break;
                case "balance_ASC":
                    query = query.OrderBy(t => t.Balance);
                    break;
                case "phone_DESC":
                    query = query.OrderByDescending(t => t.Phone);
                    break;
                case "phone_ASC":
                    query = query.OrderBy(t => t.Phone);
                    break;
                case "email_DESC":
                    query = query.OrderByDescending(t => t.Email);
                    break;
                case "email_ASC":
                    query = query.OrderBy(t => t.Email);
                    break;
                case "createdAt_DESC":
                    query = query.OrderByDescending(t => t.CreatedAt);
                    break;
                case "createdAt_ASC":
                    query = query.OrderBy(t => t.CreatedAt);
                    break;
                case "createdBy_DESC":
                    query = query.OrderByDescending(t => t.CreatedBy);
                    break;
                case "createdBy_ASC":
                    query = query.OrderBy(t => t.CreatedBy);
                    break;
            }
        }

        var data = new ListDataPagination<CustomerPerson>
        {
            Page = page,
            TotalItems = await query.CountAsync()
        };

        data.TotalPages = (int)Math.Ceiling((double)data.TotalItems / size);

        data.Data = await query
            .Skip(size * page)
            .Take(size)
            .ToListAsync();

        return data;
    }

    public async Task<CustomerPerson> GetCustomerPersonByIdAsync(UserContext user, int id)
    {
        var query = await Context.CustomerPerson
            .Where(x => x.IsDeleted == false)
            .SingleOrDefaultAsync(x => x.Id == id);

        return query;
    }

    public async Task<CustomerPerson> GetCustomerPersonByNameAsync(UserContext user, string name)
    {
        var query = await Context.CustomerPerson
            .Where(x => x.Name.ToLower() == name)
            .FirstOrDefaultAsync();

        return query;
    }

    public async Task AddCustomerPersonAsync(UserContext user, CustomerPerson customerPerson)
    {
        customerPerson.CreatedBy = user.GetCurrentUser();

        await Context.AddAsync(customerPerson);
        await Context.SaveChangesAsync();
    }
}