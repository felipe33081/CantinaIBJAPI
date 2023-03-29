using CantinaIBJ.Data.Context;
using CantinaIBJ.Data.Contracts.Customer;
using CantinaIBJ.Data.Repositories.Core;
using CantinaIBJ.Model.CustomerPerson;
using Microsoft.EntityFrameworkCore;

namespace CantinaIBJ.Data.Repositories.Customer;

public class CustomerPersonRepository : RepositoryBase<CustomerPerson>, ICustomerPersonRepository
{
    public CustomerPersonRepository(PostgreSqlContext context) : base(context)
    {

    }

    public async Task<IEnumerable<CustomerPerson>> GetCustomerPersons()
    {
        return await Context.CustomerPerson.ToListAsync();
    }

    public async Task<CustomerPerson> GetCustomerPersonByIdAsync(int id)
    {
        return await Context.CustomerPerson
            .SingleOrDefaultAsync(x => x.Id == id);
    }

    public async Task AddCustomerPersonAsync(CustomerPerson CustomerPerson)
    {
        await Context.AddAsync(CustomerPerson);
        await Context.SaveChangesAsync();
    }
}