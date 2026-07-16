using MongoDB.Driver;

namespace MongoAPI.Models.Json
{
    public class MongoContext
    {
        public IMongoDatabase Database { get; }

        public MongoContext(IMongoClient client, string DatabaseName)
        {
            Database = client.GetDatabase(DatabaseName);
        }
    }
}
