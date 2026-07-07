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
    [Route("REST/v{version:apiVersion}/Auth")]
    public class ControllerAuth : ControllerBase
    {
        private readonly Database _service;
        private readonly JwtService _jwt;
        public ControllerAuth(Database service)
        {
            _service = service;
        }

    }
}