using ElasticSearch.Models;
using ElasticSearch.Services;
using Microsoft.AspNetCore.Mvc;

namespace ElasticSearch.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly IElasticService<Product> _elasticService;
        private readonly ILogger<ProductsController> _logger;


        public ProductsController(IProductService productService, IElasticService<Product> elasticService,
                                  ILogger<ProductsController> logger)
        {
            _productService = productService;
            _elasticService = elasticService;
            _logger = logger;
        }

        [HttpPost("init")]
        public async Task<IActionResult> Initialize()
        {
            await _productService.CreateIndexAsync();

            var demoData = new[]
            {
            new Product { Id = 1, Title = "Зимние ботинки", Description = "Теплые кожаные ботинки", Price = 5000 },
            new Product { Id = 2, Title = "Летние кроссовки", Description = "Легкая обувь для бега", Price = 3000 },
            new Product { Id = 3, Title = "Компьютерная мышь", Description = "Беспроводная мышь для работы", Price = 1500 }
        };

            var success = await _productService.IndexDataAsync(demoData);

            if (success) return Ok("The index is created. Data added.");
            return BadRequest("Initialization error.");
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return BadRequest("Search request is empty.");

            var results = await _productService.SearchAsync(query);
            return Ok(results);
        }


        [HttpPost("add-product")]
        public async Task<IActionResult> AddProduct([FromBody] Product product, string indexName)
        {
            var result = await _elasticService.AddOrUpdate(product, indexName);
            return result ? Ok("Product added or updated successfully.") : StatusCode(500, "Error adding or updating Product");
        }


        [HttpPost("update-product")]
        public async Task<IActionResult> UpdateProduct([FromBody] Product product, string indexName)
        {
            var result = await _elasticService.AddOrUpdate(product, indexName);
            return result ? Ok("Product added or updated successfully.") : StatusCode(500, "Error adding or updating product");
        }

        [HttpGet("get-product/{key}")]
        public async Task<IActionResult> GetProduct(string key, string indexName)
        {
            var user = await _elasticService.Get(key, indexName);
            return user != null ? Ok(user) : StatusCode(404, "Not Found!");
        }

        [HttpGet("get-all-products")]
        public async Task<IActionResult> GetAllProducts(string indexName)
        {
            var users = await _elasticService.GetAll(indexName);
            return users != null ? Ok(users) : StatusCode(500, "Error reterving the products");
        }

        [HttpDelete("delete-product/{key}")]
        public async Task<IActionResult> DeleteProduct(string key, string indexName)
        {
            var response = await _elasticService.Remove(key, indexName);
            return response ? Ok("User deleted successfully") : StatusCode(500, "Error occcured while deleting the product.");
        }



        [HttpDelete("delete-all-products")]
        public async Task<IActionResult> DeleteProducts(string indexName)
        {
            var response = await _elasticService.RemoveAll(indexName);
            return response != null ? Ok(response) : StatusCode(500, "Error occcured while deleting the product.");
        }
    }
}
