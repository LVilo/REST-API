using System.Text.Json;

namespace MongoAPI.Models.Requests
{
    public class RequestDocument
    {
        public string DatabaseName { get; set; }
        public string CollectionName { get; set; }   
        public JsonDocument Document { get; set; }
    }
}
