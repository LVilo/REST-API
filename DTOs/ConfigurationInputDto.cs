using System.Text.Json.Serialization;

namespace RestAPI.DTOs
{
    public class ConfigurationInputDto
    {
        public string SerialNumber { get; set; } = string.Empty;
        public string DeviceName { get; set; } = string.Empty;

        [JsonExtensionData]
        public Dictionary<string, object>? ExtraFields { get; set; }
    }
}
