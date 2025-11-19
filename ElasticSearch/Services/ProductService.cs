using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Nodes;
using Elastic.Clients.Elasticsearch.QueryDsl;
using ElasticSearch.Models;

namespace ElasticSearch.Services
{
    public class ProductService : IProductService
    {
        private readonly ElasticsearchClient _client;
        private const string IndexName = "products";

        public ProductService(ElasticsearchClient client)
        {
            _client = client;
        }

        public async Task<bool> CreateIndexAsync()
        {
            if ((await _client.Indices.ExistsAsync(IndexName)).Exists)
                await _client.Indices.DeleteAsync(IndexName);

            var response = await _client.Indices.CreateAsync(IndexName, c => c
                .Mappings(m => m
                    .Properties<Product>(p => p
                        .Keyword(k => k.Id)
                        .FloatNumber(n => n.Price)
                        .Text(t => t.Title, tt => tt.Analyzer("russian"))
                        .Text(t => t.Description, td => td.Analyzer("russian"))
                    )
                )
            );

            return response.IsValidResponse;
        }

        public async Task<bool> IndexDataAsync(IEnumerable<Product> products)
        {
            var response = await _client.IndexManyAsync(products, IndexName);
            return response.IsValidResponse;
        }

        public async Task<IEnumerable<Product>> SearchAsync(string query)
        {
            //var response = await _client.SearchAsync<Product>(s => s
            //    .Index(IndexName)
            //    .Query(q => q
            //        .MultiMatch(m => m
            //            .Fields(f => f
            //                .Field(p => p.Title, boost: 2.0) // Приоритет заголовка выше
            //                .Field(p => p.Description)
            //            )
            //            .Query(query)
            //            .Fuzziness(new Fuzziness("AUTO")) // Нечеткий поиск
            //            .Operator(Operator.Or)
            //        )
            //    )
            //);


            //var response1 = await _client.SearchAsync<Product>(s => s
            //                                    .Index(IndexName)
            //                                    .Size(10)
            //                                    .Query(q => q
            //                                        .MultiMatch( mm => mm
            //                                          .Query(query)
            //                                          .Fields("Title, Description")
            //                                          .Fuzziness(new Fuzziness("AUTO"))
            //                                          .Operator(Operator.Or)
            //                                          )
            //                                    )
            //                                );

            var response = await _client.SearchAsync<Product>(s => s
                        .Index("products")
                        .Query(q => q
                            .MultiMatch(m => m
                                .Fields(new[] {
                                    Infer.Field<Product>(p => p.Title, boost: 2.0),
                                    Infer.Field<Product>(p => p.Description)
                                })
                                .Query(query)
                                .Fuzziness(new Fuzziness("AUTO"))
                            )
                        ));

            if (!response.IsValidResponse)
            {
                return Array.Empty<Product>();
            }

            return response.Documents;
        }
    }
}
