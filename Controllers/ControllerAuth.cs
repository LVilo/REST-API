using Asp.Versioning;
using DynamicData.Kernel;
using Microsoft.AspNetCore.Authorization;
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
        private readonly Database _service;
        public ControllerAuth(Database service)
        {
            _service = service;
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            var user = await _service.UserService.GetByLoginAsync(request.Login);

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
        
        [HttpPost("Registration")]
        public async Task<IActionResult> Registration(LoginRequest request)
        {
            var createduser = await _service.UserService.CreateAsync(request);

            if (createduser is null)
                return BadRequest($"Пользователь с логином \"{request.Login}\" уже существует.");

            if (!BCrypt.Net.BCrypt.Verify(request.Password, createduser.PasswordHash))
                return Unauthorized();

            var token = _jwt.GenerateToken(createduser);

            return Ok(new
            {
                token = token
            });
        }
    }
}
