using ElasticSearch.Models;
using ElasticSearch.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Reflection.Metadata.Ecma335;

namespace ElasticSearch.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly ILogger<UserController> _logger;
        private readonly IElasticService _elasticService;

        public UserController(ILogger<UserController> logger, IElasticService elasticService)
        {
            _logger = logger;
            _elasticService = elasticService;
        }


        [HttpPost("create-index")]
        public async Task<IActionResult> CreateIndex(string indexName)
        {
            await _elasticService.CreateIndexIfNotExistsAsync(indexName);
            return Ok($"Index {indexName} created or already exists.");
        }


        [HttpPost("add-user")]
        public async Task<IActionResult> AddUser([FromBody] User user)
        {
            var result = await _elasticService.AddOrUpdate(user);
            return result ? Ok("User added or updated successfully.") : StatusCode(500, "Error adding or updating User");
        }


        [HttpPost("update-user")]
        public async Task<IActionResult> UpdateUser([FromBody] User user)
        {
            var result = await _elasticService.AddOrUpdate(user);
            return result ? Ok("User added or updated successfully.") : StatusCode(500, "Error adding or updating User");
        }

        [HttpGet("get-user/{key}")]
        public async Task<IActionResult> GetUser(string key)
        {
            var user = await _elasticService.Get(key);
            return user != null ? Ok(user) : StatusCode(404, "Not Found!");
        }

        [HttpGet("get-all-user")]
        public async Task<IActionResult> GetAllUser()
        {
            var users = await _elasticService.GetAll();
            return users != null ? Ok(users) : StatusCode(500, "Error reterving the users");
        }

        [HttpDelete("delete-user/{key}")]
        public async Task<IActionResult> DeleteUser(string key)
        {
            var response = await _elasticService.Remove(key);
            return response ? Ok("User deleted successfully") : StatusCode(500, "Error occcured while deleting the user.");
        }



        [HttpDelete("delete-all-user")]
        public async Task<IActionResult> DeleteUsers()
        {
            var response = await _elasticService.RemoveAll();
            return response != null ?  Ok(response) : StatusCode(500, "Error occcured while deleting the user.");
        }
    }
}
