using CantinaIBJ.Data.Contracts.Core;
using CantinaIBJ.Model;

namespace CantinaIBJ.Data.Contracts;

public interface IProductRepository : IRepositoryBase<Product>
{
    Task<ListDataPagination<Product>> GetListProducts(UserContext contextUser,
        string searchString,
        int page,
        int size);
    Task<Product> GetProductByIdAsync(UserContext contextUser, int id);
    Task AddProductAsync(UserContext contextUser, Product product);
}