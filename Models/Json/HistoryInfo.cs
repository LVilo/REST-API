using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;

namespace MongoAPI.Models.Json
{
    public class HistoryInfo
    {
        [BsonId(IdGenerator = typeof(StringObjectIdGenerator))]
        public string? Id { get; set; }

        public string DatabaseName { get; set; }
        public string CollectionName { get; set; }
        public string IdDocument { get; set; }
        public BsonDocument Data { get; set; }
    }
}
