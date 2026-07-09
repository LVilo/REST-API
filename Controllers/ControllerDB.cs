using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using MongoAPI.Models;
using MongoAPI.Services;
using MongoDB.Bson;

namespace MongoAPI.Controllers
{
    [ApiController]
    [ApiVersion("1")]
    [Route("REST/v{version:apiVersion}/Auth")]
    public class ControllerDB : ControllerBase
    {
        private readonly Database _service;

        public ControllerDB(Database service)
        {
            _service = service;
        }
        [HttpGet("Tree")]
        public async Task<IActionResult> GetTree()
        {
            List<DBObject> tree = await _service.GetDatabasesAsync();
            if (tree.Count is 0) return NoContent();
            else return Ok(tree);
        }
        [HttpGet("documents")]
        public async Task<IActionResult> GetDocuments(DocumentQueryRequest request)
        {
            List<BsonDocument> documents = await _service.GetRecords(request);
            if (documents.Count is 0) return NoContent();
            else return Ok(documents);
        }
    }
}
