using Microsoft.VisualBasic.FileIO;

namespace MongoAPI.Models.Json
{
    public class Field
    {
        public string Name { get; set; } = null!;

        public FieldType Type { get; set; }

        public bool Required { get; set; }

        public bool Unique { get; set; }

        public object? DefaultValue { get; set; }

        public int? Min { get; set; }

        public int? Max { get; set; }

        public int? MinLength { get; set; }

        public int? MaxLength { get; set; }

        public string? Pattern { get; set; }
    }
}
