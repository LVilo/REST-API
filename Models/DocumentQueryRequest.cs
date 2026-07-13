using System.Text.Json;

namespace MongoAPI.Models
{
    public class DocumentQueryRequest
    {
        public string Database { get; set; }

        public string Collection { get; set; }

        public int Limit { get; set; } = 100;

        public List<FilterRequest>? Filters { get; set; } = [];

    }
    public class FilterRequest
    {
        public string Field { get; set; }

        public string Operator { get; set; }

        public JsonElement Value { get; set; }
    }
}
