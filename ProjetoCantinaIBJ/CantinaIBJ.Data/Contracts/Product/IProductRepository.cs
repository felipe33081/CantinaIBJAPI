using CantinaIBJ.Data.Contracts.Core;
using CantinaIBJ.Model.Product;

namespace CantinaIBJ.Data.Contracts;

public interface IProductRepository : IRepositoryBase<Product>
{
    Task<IEnumerable<Product>> GetProducts();
    Task<Product> GetProductByIdAsync(int id);
    Task AddProductAsync(Product product);
}