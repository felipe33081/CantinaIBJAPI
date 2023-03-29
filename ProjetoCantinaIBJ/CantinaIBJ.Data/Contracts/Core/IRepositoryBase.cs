namespace CantinaIBJ.Data.Contracts.Core;

public interface IRepositoryBase<T> where T : class
{
    Task<T> AddAsync(T obj);
    Task AddNoCommitAsync(T obj);
    void UpdateNoCommit(T obj);
    Task<T> GetAsync(T obj);
    Task<T> GetAsync(Guid id);
    Task<List<T>> ListAsync();
    Task DeleteAsync(T obj);
    Task DeleteAsync(int id);
    Task<T> UpdateAsync(T obj);
    Task SaveChangesAsync();
}