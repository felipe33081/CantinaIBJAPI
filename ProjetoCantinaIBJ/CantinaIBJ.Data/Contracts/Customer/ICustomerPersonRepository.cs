using CantinaIBJ.Data.Contracts.Core;
using CantinaIBJ.Model;
using CantinaIBJ.Model.Customer;

namespace CantinaIBJ.Data.Contracts.Customer;

public interface ICustomerPersonRepository : IRepositoryBase<CustomerPerson>
{
    Task<IEnumerable<CustomerPerson>> GetCustomerPersons(UserContext contextUser); 
    Task<CustomerPerson> GetCustomerPersonByIdAsync(UserContext contextUser, int id); 
    Task AddCustomerPersonAsync(UserContext contextUser, CustomerPerson customerPerson);
}