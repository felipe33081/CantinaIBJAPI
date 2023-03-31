using CantinaIBJ.Data.Contracts.Core;
using CantinaIBJ.Model.Customer;

namespace CantinaIBJ.Data.Contracts.Customer;

public interface ICustomerPersonRepository : IRepositoryBase<CustomerPerson>
{
    Task<IEnumerable<CustomerPerson>> GetCustomerPersons();
    Task<CustomerPerson> GetCustomerPersonByIdAsync(int id);
    Task AddCustomerPersonAsync(CustomerPerson customerPerson);
}