using System.Collections.Generic;
using DynamicData;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace RestAPI.Services
{
    public class MongoDeviceService
    {
        private readonly IMongoCollection<DeviceConfig> _collection;

        // connectionString пример: "mongodb://user:password@192.168.1.50:27017/?authSource=admin"
        public MongoDeviceService(string connectionString, string databaseName = "arm_configs", string collectionName = "device_configs")
        {
            var client = new MongoClient(connectionString);
            var database = client.GetDatabase(databaseName);
            _collection = database.GetCollection<DeviceConfig>(collectionName);
        }

        public async Task AddDeviceConfigAsync(DeviceConfig config)
        {
            await _collection.InsertOneAsync(config);
        }

        public async Task<DeviceConfig> GetRecordBySerialNumberAsync(uint serialNumber)
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

        public async Task<DeviceConfig> GetMaxSerialNumberByFamilyAsync(string deviceFamily)
        {
            return await _collection
                .Find(d => d.DeviceFamily == deviceFamily)
                .SortByDescending(d => d.SerialNumber)
                .FirstOrDefaultAsync();
        }

        public async Task<List<DeviceConfig>> SearchAsync(uint? serialNumber = null, string? orderNumber = null,
            string? deviceType = null, string? deviceFamily = null, string? arm = null, bool? isActual = null)
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

            return await _collection.Find(filter).ToListAsync();
        }
    }
}
