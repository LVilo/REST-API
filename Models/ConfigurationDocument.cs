using System.ComponentModel.DataAnnotations;

namespace RestAPI.Models
{
    public class ConfigurationDocument
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public string SerialNumber { get; set; } = string.Empty;

        public string DeviceName { get; set; } = string.Empty;

        public Dictionary<string, object> ExtraData { get; set; } = new();

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? DeletedAt { get; set; }
    }
}
