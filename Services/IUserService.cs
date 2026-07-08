using MongoAPI.Models;

namespace MongoAPI.Services
{
    public interface IUserService
    {
        Task<User?> GetByLoginAsync(string login);
        Task<User?> CreateAsync(LoginRequest request);
        Task<List<User>> GetALLAsync(int limit = 50);
        //Task<User> GetByLogin(string login);
        Task<bool> SetRoleAsync(string id,string role);
    }
}
