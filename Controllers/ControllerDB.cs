using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoAPI.Models;
using MongoAPI.Models.Requests;
using MongoAPI.Services;
using MongoDB.Bson;

namespace MongoAPI.Controllers
{
    [ApiController]
    [ApiVersion("1")]
    [Route("REST/v{version:apiVersion}/Collection")]
    public class ControllerDB : Controller
    {
        private readonly Database _service;

        public ControllerDB(Database service)
        {
            _service = service;
        }
        [HttpGet("Tree")]
        public async Task<IActionResult> GetTree()
        {
            var isAdmin = User.IsInRole("Admin");
            return await Try(() => _service.GetDatabasesAsync(isAdmin));

        }
        [HttpPost("Documents")]
        public async Task<IActionResult> GetDocuments(DocumentQueryRequest request)
        {
            return await Try(() => _service.GetRecords(request));
        }
        [HttpPost("Fields")]
        public async Task<IActionResult> GetFields(string collection)
        {
            return await Try(() => _service.GetFieldsAsync(collection));
        }
        [Authorize(Roles = Roles.Admin)]
        [HttpPost("Replace")]
        public async Task<IActionResult> ReplaceDocument([FromBody] RequestDocument request)
        {
            try
            {
                var result = await _service.ReplaceDocumentAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + ex.StackTrace);
                return Problem(ex.Message);
            }
        }
    }
}
