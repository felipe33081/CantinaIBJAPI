using CantinaIBJ.Data.Contracts.Core;
using CantinaIBJ.Model;
using CantinaIBJ.Model.Product;

namespace CantinaIBJ.Data.Contracts;

public interface IProductRepository : IRepositoryBase<Product>
{
    Task<IEnumerable<Product>> GetProducts(UserContext user);
    Task<Product> GetProductByIdAsync(UserContext user, int id);
    Task AddProductAsync(UserContext user, Product product);
}