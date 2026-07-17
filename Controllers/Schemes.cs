using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using MongoAPI.Models.Json;
using MongoAPI.Services;
using System.Text.Json;

namespace MongoAPI.Controllers
{
    [ApiController]
    [ApiVersion("1")]
    [Route("REST/v{version:apiVersion}/Schemes")]
    public class Schemes : Controller
    {
        private readonly Database _service;

        public Schemes(Database service)
        {
            _service = service;
        }

        [HttpGet("Single")]
        public async Task<IActionResult> Get(string Id)
        {
            return await Try(() =>_service.GetSchemaAsync(Id));
        }
        [HttpGet("All")]
        public async Task<IActionResult> Get()
        {
            return await Try(_service.GetAllSchemasAsync);
        }
        [HttpPost]
        public async Task<IActionResult> Post(Schema schema)
        {
            return await Try(() =>_service.CreateSchemaAsync(schema));
        }
        //[HttpPut]
        //public async Task<IActionResult> Put(string schema_id, Schema schema)
        //{
        //    return await Try(_service);
        //}
        //[HttpDelete]
        //public async Task<IActionResult> Delete(string schema_id)
        //{
        //    return await Try(_service);

        //}
    }
}
