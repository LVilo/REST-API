using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using System.IdentityModel;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using MongoAPI.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace MongoAPI.Services
{
    public class JwtService
    {   
        private readonly IConfiguration _configuration;

        public JwtService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerateToken(User user)
        {
            try
            {
                var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.Login),
                new Claim(ClaimTypes.Role, user.Role)
            };

                var keyBytes = Convert.FromBase64String(_configuration["Jwt:Key"]);
                var key = new SymmetricSecurityKey(keyBytes);

                var creds = new SigningCredentials(
                    key,
                    SecurityAlgorithms.HmacSha256);

                var token = new JwtSecurityToken(
                    issuer: "MongoAPI",
                    audience: "MongoAPI",
                    claims: claims,
                    expires: DateTime.Now.AddHours(1),
                    signingCredentials: creds);

                return new JwtSecurityTokenHandler().WriteToken(token);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.StackTrace); 
            }
        }
    }
}