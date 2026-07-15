using MongoDB.Bson;

namespace MongoAPI.Models
{
    public class UpdateRequest
{
    public string Database { get; set; }
    public string Collection { get; set; }
    public object Filter { get; set; }
    public object Update { get; set; }
}
}
