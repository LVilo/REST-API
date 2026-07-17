using Asp.Versioning;
using DynamicData.Kernel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoAPI.Models;
using MongoAPI.Models.Json;
using MongoAPI.Models.Requests;
using MongoAPI.Services;
using MongoDB.Bson;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace MongoAPI.Controllers.V1
{
    [ApiController]
    [ApiVersion("1")]
    [Route("REST/v{version:apiVersion}/Documents")]
    public class Documents : Controller
    {
        private readonly Database _service;

        public Documents(Database service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> Get(DocumentQueryRequest request)
        {
            return await Try(() => _service.GetRecords(request));
        }
        [HttpPost]
        public async Task<IActionResult> Post(RequestDocument request)
        {
            return await Try(() => _service.InsertDocumentAsync(request));
        }
        [HttpPut]
        public async Task<IActionResult> Put(RequestDocument request)
        {
            return await Try(() => _service.ReplaceDocumentAsync(request));
        }
        //[HttpDelete]
        //public async Task<IActionResult> Delete(string schema_id, string document_id)
        //{


        //}
    }
}

