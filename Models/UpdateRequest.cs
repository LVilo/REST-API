namespace MongoAPI.Models
{
    public class UpdateRequest
{
    public string Database { get; set; }
    public string Collection { get; set; }
    public Dictionary<string, object> Filter { get; set; }
    public Dictionary<string, object> Update { get; set; }
}
}
