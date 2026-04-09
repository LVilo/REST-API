using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using System.Text.Json;

namespace MongoAPI.Models
{
    public class Message
    {
        public string? NameDB { get; set; } = "MongoDB";
    }

    public class DeviceConfig : Message
    {
        [BsonId(IdGenerator = typeof(StringObjectIdGenerator))]
        public string? Id { get; set; } = null;

        public string Arm { get; set; }
        public string DeviceFamily { get; set; }
        public string DeviceType { get; set; }
        public ulong SerialNumber { get; set; }
        public string OrderNumber { get; set; }
        public bool IsActual { get; set; }
        public List<Settings> Settings { get; set; }
    }
    public class Settings
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }

}
