using MongoDB.Bson;

namespace MongoAPI.Models
{
    public class UpdateRequest
    {
        public string Database { get; set; }
        public string Collection { get; set; }
        public BsonDocument Filter { get; set; }
        public BsonDocument Update { get; set; }
    }
}
