using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;

namespace MongoAPI.Models.Json
{
    public class Counter
    {
        [BsonId]
        public string Id { get; set; }
        public ulong Value { get; set; }
    }
}
