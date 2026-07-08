using MongoAPI.Models;
using MongoDB.Bson;
using MongoDB.Driver;

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
        //public async Task<List<BsonDocument>> GetRecords(DocumentQueryRequest request)
        //{
        //    var database = Client.GetDatabase(request.Database);
        //    var colection = database.GetCollection<BsonDocument>(request.Colection);
        //    var filter = BuildFilter(request.Filters);
        //    return await colection
        //    .Find(filter)
        //    .Skip((request.Page-1) * request.PageSize)
        //    .Limit(request.PageSize)
        //    .ToListAsync();

        //}
        //public async Task<List<string>> GetFields(string databasename,string colectionname)
        //{
        //    var database = Client.GetDatabase(databasename);
        //    var colection = database.GetCollection<BsonDocument>(colectionname);
        //    var documents = await colection.Find(FilterDefinition<BsonDocument>.Empty).Limit(100).ToListAsync();
        //    var fields = new HashSet<string>();
        //    foreach(var document in documents)
        //    {
        //        foreach(var element in document.Elements)
        //        {
        //            fields.Add(element.Name);
        //        }
        //    }
        //    return fields.OrderBy(f=>f).ToList();
        //}
        //public FilterDefinition<BsonDocument> BuildFilter(IEnumerable<FilterRequest> filters)
        //{
        //    var builder = Builders<BsonDocument>.Filter;
        //    var filterList = new List<FilterDefinition<BsonDocument>>();

        //    foreach (var filter in filters)
        //    {
        //        var current = filter.Operator switch
        //        {
        //            "eq" => builder.Eq(filter.Field, BsonValue.Create(filter.Value)),
        //            "gt" => builder.Gt(filter.Field, BsonValue.Create(filter.Value)),
        //            "gte" => builder.Gte(filter.Field, BsonValue.Create(filter.Value)),
        //            "lt" => builder.Lt(filter.Field, BsonValue.Create(filter.Value)),
        //            "lte" => builder.Lte(filter.Field, BsonValue.Create(filter.Value)),
        //            "contains" => builder.Regex(
        //                filter.Field,
        //                new BsonRegularExpression(filter.Value.ToString(), "i")),
        //            _ => null // Skip unsupported operators
        //        };

        //        if (current != null)
        //            filterList.Add(current);
        //    }

        //    return filterList.Any() 
        //        ? builder.And(filterList) 
        //        : builder.Empty;
        //}
    }
}
