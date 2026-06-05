using MongoAPI.Models;

namespace MongoAPI.Services
{
    public interface Database
    {
        //public string ConnectionString { get; set; }
        //public string DatabaseName { get; set; }

        public Task AddDeviceConfigAsync(Config config);

        //public Task<Config> GetBySerialNumberAsync(ulong serialNumber);

        //public Task<List<Config>> GetByOrderNumberAsync(string orderNumber);

        //public Task<List<Config>> GetByDeviceFamilyAsync(string devicefamily);

        public Task<Config> GetRecordMaxSerialNumberByFamilyAsync(string deviceFamily);
        public Task<List<Config>> SearchAsync(int limit, ulong ? serialNumber = null, string orderNumber = null,
            string deviceType = null, string deviceFamily = null, string revision = null, string username = null, string date = null, string arm = null, bool? isActual = null);
        public Task<bool> PutAsync(Config config);
        public Task<bool> DeleteAsync(string Id);
        public Task<Config> GetRecordByIdAsync(string Id);
    }
}
