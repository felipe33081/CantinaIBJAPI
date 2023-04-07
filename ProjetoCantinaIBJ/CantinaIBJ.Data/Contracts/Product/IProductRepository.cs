using CantinaIBJ.Data.Contracts.Core;
using CantinaIBJ.Model;

namespace CantinaIBJ.Data.Contracts;

public interface IProductRepository : IRepositoryBase<Product>
{
    Task<List<Product>> GetProducts(UserContext contextUser);
    Task<Product> GetProductByIdAsync(UserContext contextUser, int id);
    Task AddProductAsync(UserContext contextUser, Product product);
}