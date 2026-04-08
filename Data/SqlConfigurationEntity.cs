using Microsoft.EntityFrameworkCore;
using RestAPI.Models;
using System.Text.Json;

namespace RestAPI.Data
{
    public class SqlConfigurationEntity
    {
        public Guid Id { get; set; }
        public string SerialNumber { get; set; } = string.Empty;
        public string DeviceName { get; set; } = string.Empty;
        public string ExtraDataJson { get; set; } = "{}";
        public DateTime CreatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
    }

    public class AppDbContext : DbContext
    {
        public DbSet<SqlConfigurationEntity> Configurations => Set<SqlConfigurationEntity>();

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    }
}
