using MongoAPI.Models;
using MongoDB.Driver;

namespace MongoAPI.Services
{
    public class MongoDB : Database
    {
        private readonly IMongoCollection<DeviceConfig> _collection;

        // connectionString пример: "mongodb://user:password@192.168.1.50:27017/?authSource=admin"
        public MongoDB(string connectionString, string databaseName = "arm_config", string collectionName = "Devices")
        {
            var client = new MongoClient(connectionString);
            var database = client.GetDatabase(databaseName);
            _collection = database.GetCollection<DeviceConfig>(collectionName);
        }

        public async Task AddDeviceConfigAsync(DeviceConfig config)
        {

            await _collection.InsertOneAsync(config);
        }

        public async Task<DeviceConfig> GetBySerialNumberAsync(ulong serialNumber)
        {
            return await _collection.Find(d => d.SerialNumber == serialNumber).FirstOrDefaultAsync();
        }

        public async Task<List<DeviceConfig>> GetByOrderNumberAsync(string orderNumber)
        {
            return await _collection.Find(d => d.OrderNumber == orderNumber).ToListAsync();
        }

        public async Task<List<DeviceConfig>> GetByDeviceTypeAsync(string deviceType)
        {
            return await _collection.Find(d => d.DeviceType == deviceType).ToListAsync();
        }

        public async Task<DeviceConfig> GetRecordMaxSerialNumberByFamilyAsync(string deviceFamily)
        {
            return await _collection
                .Find(d => d.DeviceFamily == deviceFamily && d.IsActual)
                .SortByDescending(d => d.SerialNumber)
                .FirstOrDefaultAsync();
        }
        public async Task<List<DeviceConfig>> SearchAsync(ulong? serialNumber = null, string orderNumber = null,
            string deviceType = null, string deviceFamily = null, string arm = null, bool? isActual = null)
        {
            var filterBuilder = Builders<DeviceConfig>.Filter;
            var filter = filterBuilder.Empty;

            if (serialNumber is not null)
                filter &= filterBuilder.Eq(d => d.SerialNumber, serialNumber);
            if (!string.IsNullOrEmpty(orderNumber))
                filter &= filterBuilder.Eq(d => d.OrderNumber, orderNumber);
            if (!string.IsNullOrEmpty(deviceType))
                filter &= filterBuilder.Eq(d => d.DeviceType, deviceType);
            if (!string.IsNullOrEmpty(deviceFamily))
                filter &= filterBuilder.Eq(d => d.DeviceFamily, deviceFamily);
            if (!string.IsNullOrEmpty(arm))
                filter &= filterBuilder.Eq(d => d.Arm, arm);
            if (isActual.HasValue)
                filter &= filterBuilder.Eq(d => d.IsActual, isActual.Value);

            List<DeviceConfig> devices = await _collection.Find(filter).ToListAsync();
            return devices[50..];
        }
    }
}
