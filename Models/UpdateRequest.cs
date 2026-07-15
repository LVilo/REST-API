using MongoDB.Bson;
using System.Text.Json;

namespace MongoAPI.Models
{
   public class UpdateRequest
    {
        public string Database { get; set; }
        public string Collection { get; set; }
        public JsonElement Filter { get; set; }
        public List<Change> Changes { get; set; }
    }

    public class Change
    {
        public string Path { get; set; }
        public JsonElement Value { get; set; }
    }
}
