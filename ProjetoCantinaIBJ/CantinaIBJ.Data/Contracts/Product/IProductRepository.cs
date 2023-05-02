using CantinaIBJ.Data.Contracts.Core;
using CantinaIBJ.Model;
using CantinaIBJ.Model.Customer;

namespace CantinaIBJ.Data.Contracts;

public interface IProductRepository : IRepositoryBase<Product>
{
    Task<int> GetCountList();
    Task<ListDataPagination<Product>> GetListProducts(UserContext contextUser, int page, int size, string? name, string? description, string? searchString, bool isDeleted, string? orderBy);
    Task<Product> GetProductByIdAsync(UserContext contextUser, int id);
    Task AddProductAsync(UserContext contextUser, Product product);
}