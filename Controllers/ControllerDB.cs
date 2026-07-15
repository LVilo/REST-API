using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoAPI.Models;
using MongoAPI.Services;
using MongoDB.Bson;

namespace MongoAPI.Controllers
{
    [ApiController]
    [ApiVersion("1")]
    [Route("REST/v{version:apiVersion}/Collection")]
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
            try
            {
                var isAdmin = User.IsInRole("Admin");
                List<DBObject> tree = await _service.GetDatabasesAsync(isAdmin);
                if (tree.Count is 0) return NoContent();
                else return Ok(tree);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + ex.StackTrace);
                return Problem(ex.Message);
            }

        }
        [HttpPost("Documents")]
        public async Task<IActionResult> GetDocuments(DocumentQueryRequest request)
        {
            try
            {
                List<BsonDocument> documents = await _service.GetRecords(request);
                if (documents.Count is 0) return NoContent();
                else
                {
                    var json = documents.Select(d => d.ToJson());
                    return Ok(json);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + ex.StackTrace);
                return Problem(ex.Message);
            }

        }
        [HttpPost("Fields")]
        public async Task<IActionResult> GetFields(DocumentQueryRequest request)
        {
            try
            {
                List<Field> Fields = await _service.GetFields(request);
                if (Fields.Count is 0) return NoContent();
                else return Ok(Fields);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + ex.StackTrace);
                return Problem(ex.Message);
            }

        }
        [Authorize(Roles = Roles.Admin)]
        [HttpPost("Update")]
        public async Task<IActionResult> UpdateDocument([FromBody] UpdateRequest request)
        {
            try
            {
                var result = await _service.Update(request.Database, request.Collection, request.Filter, request.Changes);
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
