namespace MongoAPI.Models
{
    public class DBObject
    {
        public string DatabaseName{ get; set; }
        public List<string> CollectionNames { get; set; }
        public DBObject(string name, List<string> collections)
        {
            DatabaseName = name;
            CollectionNames = collections;
        }
    }
}
