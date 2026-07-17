using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;

namespace MongoAPI.Models.Json
{
    public class CollectionInfo
    {
        [BsonId(IdGenerator = typeof(StringObjectIdGenerator))]
        public string Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string SchemaId { get; set; }

        public int SchemaVersion { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
