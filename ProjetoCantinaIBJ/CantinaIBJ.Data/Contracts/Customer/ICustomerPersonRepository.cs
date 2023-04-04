using CantinaIBJ.Data.Contracts.Core;
using CantinaIBJ.Model;
using CantinaIBJ.Model.Customer;

namespace CantinaIBJ.Data.Contracts.Customer;

public interface ICustomerPersonRepository : IRepositoryBase<CustomerPerson>
{
    Task<IEnumerable<CustomerPerson>> GetCustomerPersons(UserContext user);
    Task<CustomerPerson> GetCustomerPersonByIdAsync(UserContext user, int id);
    Task AddCustomerPersonAsync(UserContext user, CustomerPerson customerPerson);
}