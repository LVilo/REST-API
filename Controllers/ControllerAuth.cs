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
        private readonly JwtService _jwt;
        private readonly IUserService _users;
        public ControllerAuth(Database service)
        {
            _service = service;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            var user = await _users.GetByLogin(request.Login);

            if (user == null)
                return Unauthorized();

            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                return Unauthorized();

            var token = _jwt.GenerateToken(user);

            return Ok(new
            {
                token = token
            });
        }
    }
}
