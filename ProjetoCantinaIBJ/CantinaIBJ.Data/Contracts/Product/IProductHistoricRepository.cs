using CantinaIBJ.Data.Contracts.Core;
using CantinaIBJ.Model.Product;
using CantinaIBJ.Model.User;

namespace CantinaIBJ.Data.Contracts;

public interface IProductHistoricRepository : IRepositoryBase<ProductHistoric>
{
    Task<IEnumerable<ProductHistoric>> GetProductHistorics(User user);
    Task<ProductHistoric> GetProductHistoricByIdAsync(User user, int id);
    Task AddProductHistoricAsync(User user, ProductHistoric productHistoric);
}