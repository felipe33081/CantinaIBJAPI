using CantinaIBJ.Data.Contracts.Core;
using CantinaIBJ.Model;
using CantinaIBJ.Model.Product;

namespace CantinaIBJ.Data.Contracts;

public interface IProductHistoricRepository : IRepositoryBase<ProductHistoric>
{
    Task<IEnumerable<ProductHistoric>> GetProductHistorics(UserContext user);
    Task<ProductHistoric> GetProductHistoricByIdAsync(UserContext user, int id);
    Task AddProductHistoricAsync(UserContext user, ProductHistoric productHistoric);
}