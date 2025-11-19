using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Nodes;
using ElasticSearch.Configuration;
using ElasticSearch.Models;
using Microsoft.Extensions.Options;
using System.Linq;
using System.Reflection;

namespace ElasticSearch.Services
{
    public class ElasticService<T> : IElasticService<T> where T : class
    {
        private readonly ElasticsearchClient _client;
        private readonly ElasticSettings _elasticSettings;

        public ElasticService(IOptions<ElasticSettings> elasticSettings, ElasticsearchClient client)
        {
            _elasticSettings = elasticSettings.Value;
            _client = client;
        }

        public async Task<bool> AddOrUpdate(T entity, string? index)
        {
            var response = await _client.IndexAsync(entity, idx =>
            {
                idx.Index(string.IsNullOrWhiteSpace(index) ?  _elasticSettings.DefaultIndex : index)
                 .OpType(OpType.Index);
            });

            return response.IsValidResponse;
        }

        public async Task<bool> AddOrUpdateBulk(IEnumerable<T>  entities, string indexName)
        {
            var response = await _client.BulkAsync(x => x.Index(_elasticSettings.DefaultIndex)
            .UpdateMany(entities, (ud, u) => ud.Doc(u).DocAsUpsert(true)));

            return response.IsValidResponse;
        }
     
        public async Task CreateIndexIfNotExistsAsync(string indexName)
        {
            var result = await _client.Indices.ExistsAsync(indexName);

            if (!result.Exists)
            {
               await _client.Indices.CreateAsync(indexName);
            }
        }

        public async Task<T?> Get(string key, string? index)
        {
            var response = await _client.GetAsync<T>(key, g =>
            {
                g.Index(string.IsNullOrWhiteSpace(index) ? _elasticSettings.DefaultIndex : index);
            });

            return response?.Source;
        }

        public async Task<List<T>?> GetAll(string? index)
        {
            var response = await _client.SearchAsync<T>(x => x.Index(string.IsNullOrWhiteSpace(index) ? _elasticSettings.DefaultIndex : index));

            return response.IsValidResponse ? response.Documents.ToList() : default;
        }

        public async Task<bool> Remove(string key, string? index)
        {
            var response = await _client.DeleteAsync<BaseEntity>(key, x => x.Index(string.IsNullOrWhiteSpace(index) ? _elasticSettings.DefaultIndex : index));

            return response.IsValidResponse;
        }

        public async Task<long?> RemoveAll(string? index)
        {
            var response = await _client.DeleteByQueryAsync<BaseEntity>(d => d.Indices(string.IsNullOrWhiteSpace(index) ? _elasticSettings.DefaultIndex : index));

            return response.IsValidResponse ? response.Deleted : default;
        }

    }
}
