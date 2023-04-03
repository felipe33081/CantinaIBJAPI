using CantinaIBJ.Data.Contracts.Core;
using CantinaIBJ.Model.Customer;
using CantinaIBJ.Model.User;

namespace CantinaIBJ.Data.Contracts.Customer;

public interface ICustomerPersonRepository : IRepositoryBase<CustomerPerson>
{
    Task<IEnumerable<CustomerPerson>> GetCustomerPersons(User user);
    Task<CustomerPerson> GetCustomerPersonByIdAsync(User user, int id);
    Task AddCustomerPersonAsync(User user, CustomerPerson customerPerson);
}