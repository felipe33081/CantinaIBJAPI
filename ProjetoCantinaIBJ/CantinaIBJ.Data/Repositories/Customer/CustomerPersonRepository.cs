using CantinaIBJ.Data.Context;
using CantinaIBJ.Data.Contracts.Customer;
using CantinaIBJ.Data.Repositories.Core;
using CantinaIBJ.Model.Customer;
using CantinaIBJ.Model.User;
using Microsoft.EntityFrameworkCore;

namespace CantinaIBJ.Data.Repositories.Customer;

public class CustomerPersonRepository : RepositoryBase<CustomerPerson>, ICustomerPersonRepository
{
    public CustomerPersonRepository(PostgreSqlContext context) : base(context)
    {

    }

    public async Task<IEnumerable<CustomerPerson>> GetCustomerPersons(User user)
    {
        return await Context.CustomerPerson.ToListAsync();
    }

    public async Task<CustomerPerson> GetCustomerPersonByIdAsync(User user, int id)
    {
        return await Context.CustomerPerson
            .SingleOrDefaultAsync(x => x.Id == id);
    }

    public async Task AddCustomerPersonAsync(User user, CustomerPerson customerPerson)
    {
        customerPerson.CreatedBy = user.GetCurrentUser();
        customerPerson.UpdatedBy = user.GetCurrentUser();

        await Context.AddAsync(customerPerson);
        await Context.SaveChangesAsync();
    }
}