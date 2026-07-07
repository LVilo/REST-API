using MongoAPI.Models;

namespace MongoAPI.Services
{
    public interface IUserService
    {
        Task<User?> GetByLoginAsync(string login);
        Task<User?> CreateAsync(User user);
        Task<List<User>> GetALLAsync();
        Task<User> GetByLogin(string login);
    }
}
