using Asp.Versioning;
using DynamicData.Kernel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoAPI.Models;
using MongoAPI.Services;
using MongoDB.Bson;
using MongoDB.Bson.IO;
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
        public ControllerAuth(Database service, JwtService jwt)
        {
            _service = service;
            _jwt = jwt;
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            try
            {
                var user = await _service.UserService.GetByLoginAsync(request.Login);

                if (user == null)
                    return Unauthorized("Не существующий логин");

                if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                    return Unauthorized("Неверный пароль");

                var token = _jwt.GenerateToken(user);

                return Ok(new
                {
                    token = token
                });
            }
            catch (Exception ex)
            {
                return Conflict($"Необработанное исключение {{{ex.StackTrace}}}");
            }
        }

        [HttpPost("Registration")]
        public async Task<IActionResult> Registration(LoginRequest request)
        {
            try
            {
                var createduser = await _service.UserService.CreateAsync(request);

                if (createduser is null)
                    return BadRequest($"Пользователь с логином \"{request.Login}\" уже существует.");

                if (!BCrypt.Net.BCrypt.Verify(request.Password, createduser.PasswordHash))
                    return Unauthorized("Неверный пароль");

                var token = _jwt.GenerateToken(createduser);

                return Ok(new
                {
                    token = token
                });
            }
            catch (Exception ex)
            {
                return Conflict($"Необработанное исключение {{{ex.StackTrace}}}");
            }
        }
        [HttpPut("Role")]
        public async Task<IActionResult> PutRole(UpdateRoleRequest request)
        {
            if (await _service.UserService.SetRoleAsync(request.UserId, request.Role) is true) return Ok();
            else return Conflict();
        }
        [HttpGet("Users")]
        public async Task<IActionResult> GetUsers([FromQuery] string? role, [FromQuery] int limit = 50)
        {
            List<User> users = await _service.UserService.GetALLAsync(role, limit);
            if (users.Count is 0) return NoContent();
            else return Ok(users);
        }
    }
}
