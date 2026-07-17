using MongoAPI.Models;
using MongoAPI.Models.Json;
using MongoAPI.Models.Requests;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using System.Text.Json;
using static System.Net.WebRequestMethods;

namespace MongoAPI.Services
{
    public interface Database
    {
        public IUserService UserService { get; set; }
        //public string ConnectionString { get; set; }
        //public string DatabaseName { get; set; }

        //public Task AddDeviceConfigAsync(Config config);

        //public Task<Config> GetBySerialNumberAsync(ulong serialNumber);

        //public Task<List<Config>> GetByOrderNumberAsync(string orderNumber);

        //public Task<List<Config>> GetByDeviceFamilyAsync(string devicefamily);

        //public Task<Config> GetRecordMaxSerialNumberByFamilyAsync(string deviceFamily);
        //public Task<List<Config>> SearchAsync(int limit, ulong ? serialNumber = null, string orderNumber = null,
        //    string deviceType = null, string deviceFamily = null, string revision = null, string username = null, string date = null, string arm = null, bool? isActual = null);
        //public Task<bool> PutAsync(Config config);
        //public Task<bool> DeleteAsync(string Id);
        //public Task<Config> GetRecordByIdAsync(string Id);
        //public Task<List<DBObject>> GetDatabasesAsync(bool isAdmin);
        //public Task<List<BsonDocument>> GetRecords(DocumentQueryRequest request);
        //public FilterDefinition<BsonDocument> BuildFilter(IEnumerable<FilterRequest> filters);
        //public Task<List<Field>> GetFields(DocumentQueryRequest request);
        //public Task<UpdateResult> Update(string database,string collection, JsonElement filter, List<Change> update);




        public  Task<Response> GetColectionsAsync(string databasename);
        public  Task<Response> GetRecords(DocumentQueryRequest request);
        public FilterDefinition<BsonDocument> BuildFilter(IEnumerable<FilterRequest> filters);

        public  Task<Response> CreateSchemaAsync(Schema schema);
        public  Task<Response> GetSchemaAsync(string id);

        public  Task<Response> GetAllSchemasAsync();


        public  Task<Response> CreateDatabase(string name);

        public  Task<Response> CreateCollectionInfoAsync(CollectionInfo collection);

        public  Task<Response> GetCollectionInfoByNameAsync(string name);


        public  Task<Response> GetFieldsAsync(string collectionName);
        public  Task<Response> ReplaceDocumentAsync(RequestDocument request);

        public  Task<Response> InsertDocumentAsync(RequestDocument request);


    }
}
