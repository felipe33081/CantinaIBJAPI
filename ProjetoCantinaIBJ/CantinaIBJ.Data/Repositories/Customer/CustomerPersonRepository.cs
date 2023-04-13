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

    public async Task<ListDataPagination<CustomerPerson>> GetListCustomerPersons(UserContext contextUser,
        string searchString,
        int page,
        int size)
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

        var data = new ListDataPagination<CustomerPerson>
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

    public async Task<CustomerPerson> GetCustomerPersonByIdAsync(UserContext user, int id)
    {
        var query = await Context.CustomerPerson
            .Where(x => x.IsDeleted == false)
            .SingleOrDefaultAsync(x => x.Id == id);

        return query;
    }

    public async Task AddCustomerPersonAsync(UserContext user, CustomerPerson customerPerson)
    {
        customerPerson.CreatedBy = user.GetCurrentUser();

        await Context.AddAsync(customerPerson);
        await Context.SaveChangesAsync();
    }
}