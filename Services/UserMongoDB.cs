using BCrypt.Net;
using Microsoft.IdentityModel.Tokens;
using MongoAPI.Models;
using MongoDB.Driver;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Intrinsics.X86;

namespace MongoAPI.Services
{
    public class UserMongoDB : IUserService
    {
        private readonly IMongoCollection<User> _user;

        public UserMongoDB(MongoClient client, string databaseName)
        {
            var database = client.GetDatabase(databaseName);
            _user = database.GetCollection<User>("Users");
            RegAdmin();
            RegAPM();
        }
        public async Task<User?> GetByLoginAsync(string login)
        {
            return await _user.Find(d => d.Login == login).FirstOrDefaultAsync();
        }
        public async Task<User?> CreateAsync(LoginRequest request)
        {
            User? user1 = await GetByLoginAsync(request.Login);
            if (user1 is null)
            {
                User user = new User
                {
                    Role = "Spectator",
                    Login = request.Login,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
                };
                _user.InsertOne(user);
                return user;
            }
            else return null;
        }
        public async Task<List<User>> GetALLAsync(string? role,int limit = 50)
        {
            var filterBuilder = Builders<User>.Filter;
            var filter = filterBuilder.Empty;

            if (!string.IsNullOrEmpty(role))
                filter &= filterBuilder.Eq(d => d.Role, role);

            return await _user.Find(filter).Limit(limit).ToListAsync();
        }
        public async Task<bool> SetRoleAsync(string id, string role)
        {
            var filter = Builders<User>.Filter.Eq(d => d.Id, id);
            var update = Builders<User>.Update.Set(u => u.Role, role);
            var result = await _user.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }
        private async Task RegAdmin()
        {
            User? user1 = await GetByLoginAsync("Admin");
            if (user1 is null)
            {
                User user = new User
                {
                    Role = "Admin",
                    Login = "Admin",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("123123")
                };
                _user.InsertOne(user);
            }
        }
        private async Task RegAPM()
        {
            User? user2 = await GetByLoginAsync("APM");
            if (user2 is null)
            {
                User user = new User
                {
                    Role = "APM",
                    Login = "APM",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("DCBA")
                };
                _user.InsertOne(user);
            }
        }
    }
}
