using CantinaIBJ.Data.Contracts.Core;
using CantinaIBJ.Model;
using CantinaIBJ.Model.Customer;

namespace CantinaIBJ.Data.Contracts.Customer;

public interface ICustomerPersonRepository : IRepositoryBase<CustomerPerson>
{
    Task<ListDataPagination<CustomerPerson>> GetListCustomerPersons(UserContext contextUser,
        string searchString,
        int page,
        int size);
    Task<CustomerPerson> GetCustomerPersonByIdAsync(UserContext contextUser, int id); 
    Task AddCustomerPersonAsync(UserContext contextUser, CustomerPerson customerPerson);
}