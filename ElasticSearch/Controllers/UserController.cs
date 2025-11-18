using ElasticSearch.Models;
using ElasticSearch.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Reflection.Metadata.Ecma335;

namespace ElasticSearch.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IElasticService<User> _elasticService;

        public UserController(IElasticService<User> elasticService)
        {
            _elasticService = elasticService;
        }


        [HttpPost("create-index")]
        public async Task<IActionResult> CreateIndex(string indexName)
        {
            await _elasticService.CreateIndexIfNotExistsAsync(indexName);
            return Ok($"Index {indexName} created or already exists.");
        }


        [HttpPost("add-user")]
        public async Task<IActionResult> AddUser([FromBody] User user, string indexName)
        {
            var result = await _elasticService.AddOrUpdate(user, indexName);
            return result ? Ok("User added or updated successfully.") : StatusCode(500, "Error adding or updating User");
        }


        [HttpPost("update-user")]
        public async Task<IActionResult> UpdateUser([FromBody] User user, string indexName)
        {
            var result = await _elasticService.AddOrUpdate(user, indexName);
            return result ? Ok("User added or updated successfully.") : StatusCode(500, "Error adding or updating User");
        }

        [HttpGet("get-user/{key}")]
        public async Task<IActionResult> GetUser(string key, string indexName)
        {
            var user = await _elasticService.Get(key, indexName);
            return user != null ? Ok(user) : StatusCode(404, "Not Found!");
        }

        [HttpGet("get-all-user")]
        public async Task<IActionResult> GetAllUser(string indexName)
        {
            Log.Information("Custom Log GetAllUser");
            var users = await _elasticService.GetAll(indexName);
            return users != null ? Ok(users) : StatusCode(500, "Error reterving the users");
        }

        [HttpDelete("delete-user/{key}")]
        public async Task<IActionResult> DeleteUser(string key, string indexName)
        {
            var response = await _elasticService.Remove(key, indexName);
            return response ? Ok("User deleted successfully") : StatusCode(500, "Error occcured while deleting the user.");
        }



        [HttpDelete("delete-all-user")]
        public async Task<IActionResult> DeleteUsers(string indexName)
        {
            var response = await _elasticService.RemoveAll(indexName);
            return response != null ?  Ok(response) : StatusCode(500, "Error occcured while deleting the user.");
        }
    }
}
