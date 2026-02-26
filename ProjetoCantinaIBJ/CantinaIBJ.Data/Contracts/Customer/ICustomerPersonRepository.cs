using CantinaIBJ.Data.Contracts.Core;
using CantinaIBJ.Model;
using CantinaIBJ.Model.Customer;

namespace CantinaIBJ.Data.Contracts.Customer;

public interface ICustomerPersonRepository : IRepositoryBase<CustomerPerson>
{
    Task<int> GetCountList();
    Task<ListDataPagination<CustomerPerson>> GetListCustomerPersons(int page, int size, string? name, string? email, string? searchString, bool isDeleted, string? orderBy);
    Task<CustomerPerson> GetCustomerPersonByIdAsync(int id);
    Task<CustomerPerson> GetCustomerPersonByNameAsync(string name);
    Task AddCustomerPersonAsync(CustomerPerson customerPerson);
}