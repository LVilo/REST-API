using RestAPI.Models;
namespace RestAPI.Repositories
{
    public interface IConfigurationRepository
    {
        Task<(IEnumerable<ConfigurationDocument> Items, int Total)> GetAllAsync(
            int limit,
            int offset,
            string sort,
            string? nameLike,
            string? serialFrom,
            string? serialTo,
            DateTime? dateFrom,
            DateTime? dateTo);

        Task<ConfigurationDocument?> GetByIdAsync(Guid id);

        Task<ConfigurationDocument> CreateAsync(ConfigurationDocument config);

        Task<ConfigurationDocument?> UpdateAsync(Guid id, ConfigurationDocument config);

        Task<bool> SoftDeleteAsync(Guid id);
    }
}
