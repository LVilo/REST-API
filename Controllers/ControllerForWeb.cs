using Asp.Versioning;
using DynamicData.Kernel;
using Microsoft.AspNetCore.Mvc;
using MongoAPI.Models;
using MongoAPI.Services;
using MongoDB.Bson;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Threading.Tasks;

namespace MongoAPI.Controllers.V1
{
    [ApiController]
    [ApiVersion("1")]
    [Route("REST/v{version:apiVersion}/Database")]
    public class ControllerForWeb : ControllerBase
    {
        private readonly Database _service;

        public ControllerForWeb(Database service)
        {
            _service = service;
        }
        [HttpGet]
        public async Task<IActionResult> GetDatabases()
        {
            List<DBObject> databases = await _service.GetDatabasesAsync();
            if(databases.Count is 0) return NotFound("Не найдено");
            else return Ok(databases);
        }
        [HttpGet("Records")]
        public async Task<IActionResult> GetRecords(DocumentQueryRequest request)
        {
            List<BsonDocument> Records = await _service.GetRecords(request);
            if(Records.Count is 0) return NotFound("Не найдено");
            else return Ok(Records);
        }
        [HttpGet("Fields")]
        public async Task<IActionResult> GetRecords([FromBody]string databasename,[FromBody]string colectionname)
        {
            List<string> fields = await _service.GetFields(databasename,colectionname);
            if(fields.Count is 0) return NotFound("Не найдено");
            else return Ok(fields);
        }
    }
}