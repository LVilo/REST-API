using Microsoft.EntityFrameworkCore;
using RestAPI.Data;
using RestAPI.Models;
using System.Text.Json;

namespace RestAPI.Repositories
{
    public class SqlConfigurationRepository : IConfigurationRepository
    {
        private readonly AppDbContext _context;

        public SqlConfigurationRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<(IEnumerable<ConfigurationDocument> Items, int Total)> GetAllAsync(
            int limit, int offset, string sort,
            string? nameLike, string? serialFrom, string? serialTo,
            DateTime? dateFrom, DateTime? dateTo)
        {
            var query = _context.Configurations
                .Where(x => x.DeletedAt == null)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(nameLike))
                query = query.Where(x => x.DeviceName.Contains(nameLike));

            if (!string.IsNullOrWhiteSpace(serialFrom))
                query = query.Where(x => string.Compare(x.SerialNumber, serialFrom) >= 0);

            if (!string.IsNullOrWhiteSpace(serialTo))
                query = query.Where(x => string.Compare(x.SerialNumber, serialTo) <= 0);

            if (dateFrom.HasValue)
                query = query.Where(x => x.CreatedAt >= dateFrom.Value);

            if (dateTo.HasValue)
                query = query.Where(x => x.CreatedAt <= dateTo.Value);

            query = sort switch
            {
                "createdAt" => query.OrderBy(x => x.CreatedAt),
                "-createdAt" => query.OrderByDescending(x => x.CreatedAt),
                "deviceName" => query.OrderBy(x => x.DeviceName),
                "-deviceName" => query.OrderByDescending(x => x.DeviceName),
                _ => query.OrderByDescending(x => x.CreatedAt)
            };

            var total = await query.CountAsync();

            var items = await query
                .Skip(offset)
                .Take(limit)
                .ToListAsync();

            return (
                items.Select(MapToDocument),
                total
            );
        }

        public async Task<ConfigurationDocument?> GetByIdAsync(Guid id)
        {
            var entity = await _context.Configurations
                .FirstOrDefaultAsync(x => x.Id == id && x.DeletedAt == null);

            return entity == null ? null : MapToDocument(entity);
        }

        public async Task<ConfigurationDocument> CreateAsync(ConfigurationDocument config)
        {
            var entity = MapToEntity(config);
            _context.Configurations.Add(entity);
            await _context.SaveChangesAsync();
            return MapToDocument(entity);
        }

        public async Task<ConfigurationDocument?> UpdateAsync(Guid id, ConfigurationDocument config)
        {
            var entity = await _context.Configurations
                .FirstOrDefaultAsync(x => x.Id == id && x.DeletedAt == null);

            if (entity == null) return null;

            entity.SerialNumber = config.SerialNumber;
            entity.DeviceName = config.DeviceName;
            entity.ExtraDataJson = JsonSerializer.Serialize(config.ExtraData);

            await _context.SaveChangesAsync();
            return MapToDocument(entity);
        }

        public async Task<bool> SoftDeleteAsync(Guid id)
        {
            var entity = await _context.Configurations
                .FirstOrDefaultAsync(x => x.Id == id && x.DeletedAt == null);

            if (entity == null) return false;

            entity.DeletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        private static ConfigurationDocument MapToDocument(SqlConfigurationEntity entity)
        {
            return new ConfigurationDocument
            {
                Id = entity.Id,
                SerialNumber = entity.SerialNumber,
                DeviceName = entity.DeviceName,
                CreatedAt = entity.CreatedAt,
                DeletedAt = entity.DeletedAt,
                ExtraData = JsonSerializer.Deserialize<Dictionary<string, object>>(entity.ExtraDataJson) ?? new()
            }
            ;
        }

        private static SqlConfigurationEntity MapToEntity(ConfigurationDocument doc)
        {
            return new SqlConfigurationEntity
            {
                Id = doc.Id,
                SerialNumber = doc.SerialNumber,
                DeviceName = doc.DeviceName,
                CreatedAt = doc.CreatedAt,
                DeletedAt = doc.DeletedAt,
                ExtraDataJson = JsonSerializer.Serialize(doc.ExtraData)
            };
        }
    }
}

