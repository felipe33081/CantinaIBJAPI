using CantinaIBJ.Data.Contracts.Core;
using CantinaIBJ.Model;
using CantinaIBJ.Model.Product;

namespace CantinaIBJ.Data.Contracts;

public interface IProductRepository : IRepositoryBase<Product>
{
    Task<IEnumerable<Product>> GetProducts(UserContext contextUser);
    Task<Product> GetProductByIdAsync(UserContext contextUser, int id);
    Task AddProductAsync(UserContext contextUser, Product product);
}