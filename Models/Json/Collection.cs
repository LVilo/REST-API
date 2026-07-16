using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;

namespace MongoAPI.Models.Json
{
    public class Collection
    {
        [BsonId(IdGenerator = typeof(ObjectIdGenerator))]
        public ObjectId Id { get; set; }

        public string Name { get; set; } = null!;

        public ObjectId SchemaId { get; set; }
    }
}
