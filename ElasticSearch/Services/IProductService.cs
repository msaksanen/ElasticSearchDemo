using ElasticSearch.Models;

namespace ElasticSearch.Services
{
    public interface IProductService
    {
        Task<bool> CreateIndexAsync();
        Task<bool> IndexDataAsync(IEnumerable<Product> products);
        Task<IEnumerable<Product>> SearchAsync(string query);
    }
}
