using MongoAPI.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace MongoAPI.Services
{
    public class Mongo : Database
    {
        private readonly IMongoCollection<Config> _collection;

        // connectionString пример: "mongodb://user:password@192.168.1.50:27017/?authSource=admin"
        public Mongo(string connectionString, string databaseName = "arm_config", string collectionName = "Devices")
        {
            var client = new MongoClient(connectionString);
            var database = client.GetDatabase(databaseName);
            _collection = database.GetCollection<Config>(collectionName);
        }

        public async Task AddDeviceConfigAsync(Config config)
        {

            await _collection.InsertOneAsync(config);
        }

        //public async Task<Config> GetBySerialNumberAsync(ulong serialNumber)
        //{
        //    return await _collection.Find(d => d.SerialNumber == serialNumber).FirstOrDefaultAsync();
        //}

        //public async Task<List<Config>> GetByOrderNumberAsync(string orderNumber)
        //{
        //    return await _collection.Find(d => d.OrderNumber == orderNumber).ToListAsync();
        //}

        //public async Task<List<Config>> GetByDeviceFamilyAsync(string devicefamily)
        //{
        //    return await _collection.Find(d => d.DeviceFamily == devicefamily && d.IsActual).ToListAsync();
        //}

        public async Task<Config> GetRecordMaxSerialNumberByFamilyAsync(string deviceFamily)
        {
            return await _collection
                .Find(d => d.DeviceFamily == deviceFamily && d.IsActual)
                .SortByDescending(d => d.SerialNumber)
                .FirstOrDefaultAsync();
        }
        public async Task<List<Config>> SearchAsync(int limit, ulong? serialNumber = null, string orderNumber = null,
            string deviceType = null, string deviceFamily = null, string username = null, string date = null, string arm = null, bool? isActual = null)
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
            if (!string.IsNullOrEmpty(username))
                filter &= filterBuilder.Eq(d => d.UserName, username);
            if (!string.IsNullOrEmpty(date))
                filter &= filterBuilder.Eq(d => d.Date, date);
            if (!string.IsNullOrEmpty(arm))
                filter &= filterBuilder.Eq(d => d.Arm, arm);
            if (isActual.HasValue)
                filter &= filterBuilder.Eq(d => d.IsActual, isActual.Value);

           return await _collection.Find(filter).Limit(limit).ToListAsync();
        }
        public async Task PutAsync(string ID, Config config)
        {
            var filter = Builders<Config>.Filter.Eq(d => d.Id, ID);

            //var update = Builders<Config>.Update.Set(,config);
            config.Id = ID;
            //await _collection.UpdateOneAsync(filter, config);
           var result = await _collection.ReplaceOneAsync(filter, config);
        }
        public async Task DeleteAsync(string ID)
        {
            var filter = Builders<Config>.Filter.Eq(d => d.Id, ID);
            var update = Builders<Config>.Update.Set(u => u.IsActual , false);
            await _collection.UpdateOneAsync(filter, update);
        }
        public Task<Config> GetRecordByIdAsync(string Id)
        {
            var filter = Builders<Config>.Filter.Eq(d => d.Id, Id);
            return _collection.Find(filter).FirstAsync();
        }
    }
}
