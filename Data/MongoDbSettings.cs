using MongoDB.Driver;
using RestAPI.Models;

namespace RestAPI.Data
{
    public class MongoDbSettings
    {
        public string ConnectionString { get; set; } = string.Empty;
        public string DatabaseName { get; set; } = string.Empty;
        public string CollectionName { get; set; } = string.Empty;
    }

    public class MongoDbContext
    {
        public IMongoCollection<ConfigurationDocument> Configurations { get; }

        public MongoDbContext(IConfiguration configuration)
        {
            var settings = configuration.GetSection("MongoDbSettings").Get<MongoDbSettings>();
            var client = new MongoClient(settings!.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);

            Configurations = database.GetCollection<ConfigurationDocument>(settings.CollectionName);
        }
    }
}
