using MongoAPI.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using System.Text.Json;

namespace MongoAPI.Services
{
    public class Mongo : Database
    {
        private readonly IMongoCollection<Config> _devices;
        public IUserService UserService { get; set; }
        private MongoClient Client {get;set;}

        // connectionString пример: "mongodb://user:password@192.168.1.50:27017/?authSource=admin"
        public Mongo(string connectionString, string databaseName = "arm_config", string collectionName = "Devices")
        {
            Client = new MongoClient(connectionString);
            var database = Client.GetDatabase(databaseName);
            _devices = database.GetCollection<Config>(collectionName);
            UserService = new UserMongoDB(Client, databaseName);
        }

        public async Task AddDeviceConfigAsync(Config config)
        {
            await _devices.InsertOneAsync(config);
        }

        //public async Task<Config> GetBySerialNumberAsync(ulong serialNumber)
        //{
        //    return await _devices.Find(d => d.SerialNumber == serialNumber).FirstOrDefaultAsync();
        //}

        //public async Task<List<Config>> GetByOrderNumberAsync(string orderNumber)
        //{
        //    return await _devices.Find(d => d.OrderNumber == orderNumber).ToListAsync();
        //}

        //public async Task<List<Config>> GetByDeviceFamilyAsync(string devicefamily)
        //{
        //    return await _devices.Find(d => d.DeviceFamily == devicefamily && d.IsActual).ToListAsync();
        //}

        public async Task<Config> GetRecordMaxSerialNumberByFamilyAsync(string deviceFamily)
        {
            return await _devices
                .Find(d => d.DeviceFamily == deviceFamily && d.IsActual)
                .SortByDescending(d => d.SerialNumber)
                .FirstOrDefaultAsync();
        }
        public async Task<List<Config>> SearchAsync(int limit, ulong? serialNumber = null, string orderNumber = null,
            string deviceType = null, string deviceFamily = null, string revision = null, string username = null, string date = null, string arm = null, bool? isActual = null)
        {
            var filterBuilder = Builders<Config>.Filter;
            var filter = filterBuilder.Empty;

            if (serialNumber is not null)
                filter &= filterBuilder.Eq(d => d.SerialNumber, serialNumber);
            if (!string.IsNullOrEmpty(orderNumber))
                filter &= filterBuilder.Eq(d => d.OrderNumber, orderNumber);
            if (!string.IsNullOrEmpty(deviceType))
                filter &= filterBuilder.Eq(d => d.DeviceType, deviceType);
            if (!string.IsNullOrEmpty(deviceFamily))
                filter &= filterBuilder.Eq(d => d.DeviceFamily, deviceFamily);
            if (!string.IsNullOrEmpty(revision))
                filter &= filterBuilder.Eq(d => d.Revision, revision);
            if (!string.IsNullOrEmpty(username))
                filter &= filterBuilder.Eq(d => d.UserName, username);
            if (!string.IsNullOrEmpty(date))
                filter &= filterBuilder.Eq(d => d.Date, date);
            if (!string.IsNullOrEmpty(arm))
                filter &= filterBuilder.Eq(d => d.Arm, arm);
            if (isActual.HasValue)
                filter &= filterBuilder.Eq(d => d.IsActual, isActual.Value);
            Console.WriteLine(filter.ToJson());
           return await _devices.Find(filter).Limit(limit).ToListAsync();
        }
        public async Task<bool> PutAsync(Config config)
        {
            var filter = Builders<Config>.Filter.Eq(d => d.Id, config.Id);

            //var update = Builders<Config>.Update.Set(,config);
            //config.Id = ID;
            //await _devices.UpdateOneAsync(filter, config);
           var result = await _devices.ReplaceOneAsync(filter, config);
            return result.ModifiedCount > 0;
        }
        public async Task<bool> DeleteAsync(string ID)
        {
            var filter = Builders<Config>.Filter.Eq(d => d.Id, ID);
            var update = Builders<Config>.Update.Set(u => u.IsActual , false);
            var result = await _devices.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }
        public Task<Config> GetRecordByIdAsync(string Id)
        {
            var filter = Builders<Config>.Filter.Eq(d => d.Id, Id);
            return _devices.Find(filter).FirstAsync();
        }
        public async Task<List<DBObject>> GetDatabasesAsync()
        {
            var cursor = await Client.ListDatabaseNamesAsync();
            List<string> databases = await cursor.ToListAsync();
            if(databases.Count > 0)
            {
                List<DBObject> dBObjects = new List<DBObject>();
                foreach(var item in databases)
                {
                    List<string> colections = await GetColectionsAsync(item);
                    dBObjects.Add(new DBObject(item,colections));
                }
                return dBObjects;
            }
            else
            {
                return new List<DBObject> {};
            }
        }
        public async Task<List<string>> GetColectionsAsync(string databasename)
        {
            var database = Client.GetDatabase(databasename);
            var cursor = await database.ListCollectionNamesAsync();
            return await cursor.ToListAsync();
        }
        public async Task<List<BsonDocument>> GetRecords(DocumentQueryRequest request)
        {
            var database = Client.GetDatabase(request.Database);
            var colection = database.GetCollection<BsonDocument>(request.Collection);
            var filter = BuildFilter(request.Filters);

            List<BsonDocument> documents = await colection
            .Find(filter)
            .Limit(request.Limit)
            .ToListAsync();
            if (documents.Count is 0)
            {
                var bsonFilter = filter.ToBsonDocument();
                Console.WriteLine($"фильтр - {bsonFilter.ToJson()}");
                Console.WriteLine($"документы - {documents.Any().ToJson()}");
            }
            return documents;

        }
        public async Task<List<string>> GetFields(DocumentQueryRequest request)
        {
            var database = Client.GetDatabase(request.Database);
            var colection = database.GetCollection<BsonDocument>(request.Collection);
            var documents = await colection.Find(FilterDefinition<BsonDocument>.Empty).Limit(100).ToListAsync();
            var fields = new HashSet<string>();
            foreach (var document in documents)
            {
                foreach (var element in document.Elements)
                {
                    fields.Add(element.Name);
                }
            }
            return fields.OrderBy(f => f).ToList();
        }
        public FilterDefinition<BsonDocument> BuildFilter(IEnumerable<FilterRequest> filters)
        {
            var builder = Builders<BsonDocument>.Filter;
            var filterList = new List<FilterDefinition<BsonDocument>>();

            foreach (var filter in filters)
            {
                var current = filter.Operator switch
                {
                    "eq" => builder.Eq(filter.Field, ToBsonValue(filter.Value)),
                    "gt" => builder.Gt(filter.Field, ToBsonValue(filter.Value)),
                    "gte" => builder.Gte(filter.Field, ToBsonValue(filter.Value)),
                    "lt" => builder.Lt(filter.Field, ToBsonValue(filter.Value)),
                    "lte" => builder.Lte(filter.Field, ToBsonValue(filter.Value)),
                    "contains" => builder.Regex(
                        filter.Field,
                        new BsonRegularExpression(filter.Value.ToString(), "i")),
                    _ => null // Skip unsupported operators
                };
                var rendered = current.ToBsonDocument();   // метод расширения
                Console.WriteLine(rendered.ToJson());
                if (current != null)
                    filterList.Add(current);
            }

            return filterList.Any()
                ? builder.And(filterList)
                : builder.Empty;
        }
        private static BsonValue ToBsonValue(JsonElement value)
        {
            return value.ValueKind switch
            {
                JsonValueKind.String => new BsonString(value.GetString()!),

                JsonValueKind.Number when value.TryGetInt32(out var i)
                    => new BsonInt32(i),

                JsonValueKind.Number when value.TryGetInt64(out var l)
                    => new BsonInt64(l),

                JsonValueKind.Number
                    => new BsonDouble(value.GetDouble()),

                JsonValueKind.True => BsonBoolean.True,

                JsonValueKind.False => BsonBoolean.False,

                JsonValueKind.Null => BsonNull.Value,

                _ => new BsonString(value.ToString())
            };
        }
    }
}
