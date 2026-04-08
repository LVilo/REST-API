using MongoDB.Driver;
using RestAPI.Data;
using RestAPI.Models;

namespace RestAPI.Repositories
{
    public class MongoConfigurationRepository : IConfigurationRepository
    {
        private readonly MongoDbContext _context;

        public MongoConfigurationRepository(MongoDbContext context)
        {
            _context = context;
        }

        public async Task<(IEnumerable<ConfigurationDocument> Items, int Total)> GetAllAsync(
            int limit, int offset, string sort,
            string? nameLike, string? serialFrom, string? serialTo,
            DateTime? dateFrom, DateTime? dateTo)
        {
            var filter = Builders<ConfigurationDocument>.Filter.Eq(x => x.DeletedAt, null);

            if (!string.IsNullOrWhiteSpace(nameLike))
                filter &= Builders<ConfigurationDocument>.Filter.Regex(x => x.DeviceName, new MongoDB.Bson.BsonRegularExpression(nameLike, "i"));

            if (!string.IsNullOrWhiteSpace(serialFrom))
                filter &= Builders<ConfigurationDocument>.Filter.Gte(x => x.SerialNumber, serialFrom);

            if (!string.IsNullOrWhiteSpace(serialTo))
                filter &= Builders<ConfigurationDocument>.Filter.Lte(x => x.SerialNumber, serialTo);

            if (dateFrom.HasValue)
                filter &= Builders<ConfigurationDocument>.Filter.Gte(x => x.CreatedAt, dateFrom.Value);

            if (dateTo.HasValue)
                filter &= Builders<ConfigurationDocument>.Filter.Lte(x => x.CreatedAt, dateTo.Value);

            var sortDef = sort switch
            {
                "createdAt" => Builders<ConfigurationDocument>.Sort.Ascending(x => x.CreatedAt),
                "-createdAt" => Builders<ConfigurationDocument>.Sort.Descending(x => x.CreatedAt),
                "deviceName" => Builders<ConfigurationDocument>.Sort.Ascending(x => x.DeviceName),
                "-deviceName" => Builders<ConfigurationDocument>.Sort.Descending(x => x.DeviceName),
                _ => Builders<ConfigurationDocument>.Sort.Descending(x => x.CreatedAt)
            };

            var total = (int)await _context.Configurations.CountDocumentsAsync(filter);

            var items = await _context.Configurations
                .Find(filter)
                .Sort(sortDef)
                .Skip(offset)
                .Limit(limit)
                .ToListAsync();

            return (items, total);
        }

        public async Task<ConfigurationDocument?> GetByIdAsync(Guid id)
        {
            return await _context.Configurations
                .Find(x => x.Id == id && x.DeletedAt == null)
                .FirstOrDefaultAsync();
        }

        public async Task<ConfigurationDocument> CreateAsync(ConfigurationDocument config)
        {
            await _context.Configurations.InsertOneAsync(config);
            return config;
        }

        public async Task<ConfigurationDocument?> UpdateAsync(Guid id, ConfigurationDocument config)
        {
            config.Id = id;

            var result = await _context.Configurations.ReplaceOneAsync(
                x => x.Id == id && x.DeletedAt == null,
                config);

            if (result.MatchedCount == 0)
                return null;

            return config;
        }

        public async Task<bool> SoftDeleteAsync(Guid id)
        {
            var update = Builders<ConfigurationDocument>.Update.Set(x => x.DeletedAt, DateTime.UtcNow);

            var result = await _context.Configurations.UpdateOneAsync(
                x => x.Id == id && x.DeletedAt == null,
                update);

            return result.MatchedCount > 0;
        }
    }
}
