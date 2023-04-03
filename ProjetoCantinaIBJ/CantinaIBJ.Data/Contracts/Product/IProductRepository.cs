using CantinaIBJ.Data.Contracts.Core;
using CantinaIBJ.Model.Product;
using CantinaIBJ.Model.User;

namespace CantinaIBJ.Data.Contracts;

public interface IProductRepository : IRepositoryBase<Product>
{
    Task<IEnumerable<Product>> GetProducts(User user);
    Task<Product> GetProductByIdAsync(User user, int id);
    Task AddProductAsync(User user, Product product);
}