using ElasticSearch.Models;

namespace ElasticSearch.Services
{
    public interface IElasticService<T>  where T : class 
    {
        Task CreateIndexIfNotExistsAsync(string indexName);
       
        Task<bool> AddOrUpdate(T entity, string? index);

        Task<bool> AddOrUpdateBulk(IEnumerable<T> entities, string indexName);

        Task<T?> Get(string key, string? index);

        Task<List<T>?> GetAll(string? index);

        Task<bool> Remove(string key, string? index);

        Task<long?> RemoveAll(string? index);
    }
}
