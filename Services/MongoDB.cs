using MongoAPI.Controllers;
using MongoAPI.Models;
using MongoAPI.Models.Json;
using MongoAPI.Models.Requests;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using System.Text.Json;
using static System.Net.WebRequestMethods;

namespace MongoAPI.Services
{
    public class Mongo : Database
    {
        private readonly IMongoDatabase Database;
        private readonly IMongoDatabase System;
        //private readonly IMongoCollection<Config> _devices;
        private readonly IMongoCollection<Schema> Schemas;
        private readonly IMongoCollection<CollectionInfo> Collections;

        private readonly IMongoCollection<HistoryInfo> History;


        public IUserService UserService { get; set; }
        private MongoClient Client { get; set; }

        // connectionString пример: "mongodb://user:password@192.168.1.50:27017/?authSource=admin"
        public Mongo(string connectionString)
        {
            Client = new MongoClient(connectionString);
            System = Client.GetDatabase("System");
            Database = Client.GetDatabase("Main");
            //_devices = _database.GetCollection<Config>("Devices");
            Schemas = System.GetCollection<Schema>("Schemas");
            Collections = System.GetCollection<CollectionInfo>("CollectionsInfo");
            History = System.GetCollection<HistoryInfo>("HistoriesInfo");

            UserService = new UserMongoDB(System);
        }

        //public async Task AddDeviceConfigAsync(Config config)
        //{
        //    await _devices.InsertOneAsync(config);
        //}

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

        //public async Task<Config> GetRecordMaxSerialNumberByFamilyAsync(string deviceFamily)
        //{
        //    return await _devices
        //        .Find(d => d.DeviceFamily == deviceFamily && d.IsActual)
        //        .SortByDescending(d => d.SerialNumber)
        //        .FirstOrDefaultAsync();
        //}
        //public async Task<List<Config>> SearchAsync(int limit, ulong? serialNumber = null, string orderNumber = null,
        //    string deviceType = null, string deviceFamily = null, string revision = null, string username = null, string date = null, string arm = null, bool? isActual = null)
        //{
        //    var filterBuilder = Builders<Config>.Filter;
        //    var filter = filterBuilder.Empty;

        //    if (serialNumber is not null)
        //        filter &= filterBuilder.Eq(d => d.SerialNumber, serialNumber);
        //    if (!string.IsNullOrEmpty(orderNumber))
        //        filter &= filterBuilder.Eq(d => d.OrderNumber, orderNumber);
        //    if (!string.IsNullOrEmpty(deviceType))
        //        filter &= filterBuilder.Eq(d => d.DeviceType, deviceType);
        //    if (!string.IsNullOrEmpty(deviceFamily))
        //        filter &= filterBuilder.Eq(d => d.DeviceFamily, deviceFamily);
        //    if (!string.IsNullOrEmpty(revision))
        //        filter &= filterBuilder.Eq(d => d.Revision, revision);
        //    if (!string.IsNullOrEmpty(username))
        //        filter &= filterBuilder.Eq(d => d.UserName, username);
        //    if (!string.IsNullOrEmpty(date))
        //        filter &= filterBuilder.Eq(d => d.Date, date);
        //    if (!string.IsNullOrEmpty(arm))
        //        filter &= filterBuilder.Eq(d => d.Arm, arm);
        //    if (isActual.HasValue)
        //        filter &= filterBuilder.Eq(d => d.IsActual, isActual.Value);

        //    var serializerRegistry = BsonSerializer.SerializerRegistry;
        //    var documentSerializer = serializerRegistry.GetSerializer<Config>(); // Config — ваш класс

        //    // Рендерим фильтр в BsonDocument с помощью RenderArgs
        //    var renderedFilter = filter.Render(new RenderArgs<Config>(documentSerializer, serializerRegistry));
        //    Console.WriteLine(renderedFilter.ToJson());

        //    return await _devices.Find(filter).Limit(limit).ToListAsync();
        //}
        //public async Task<bool> PutAsync(Config config)
        //{
        //    var filter = Builders<Config>.Filter.Eq(d => d.Id, config.Id);

        //    //var update = Builders<Config>.Update.Set(,config);
        //    //config.Id = ID;
        //    //await _devices.UpdateOneAsync(filter, config);
        //    var result = await _devices.ReplaceOneAsync(filter, config);
        //    return result.ModifiedCount > 0;
        //}
        //public async Task<bool> DeleteAsync(string ID)
        //{
        //    var filter = Builders<Config>.Filter.Eq(d => d.Id, ID);
        //    var update = Builders<Config>.Update.Set(u => u.IsActual, false);
        //    var result = await _devices.UpdateOneAsync(filter, update);
        //    return result.ModifiedCount > 0;
        //}
        //public Task<Config> GetRecordByIdAsync(string Id)
        //{
        //    var filter = Builders<Config>.Filter.Eq(d => d.Id, Id);
        //    return _devices.Find(filter).FirstAsync();
        //}
        //public async Task<List<DBObject>> GetDatabasesAsync(bool isAdmin)
        //{
        //    var cursor = await Client.ListDatabaseNamesAsync();
        //    List<string> databases = await cursor.ToListAsync();
        //    if (databases.Count > 0)
        //    {
        //        List<DBObject> dBObjects = new List<DBObject>();
        //        foreach (var item in databases)
        //        {
        //            if ((item is "admin" || item is "config" || item is "local") && isAdmin is false) continue;
        //            List<string> colections = await GetColectionsAsync(item);
        //            if (colections.Contains("Users") && isAdmin is false) colections.Remove("Users");
        //            dBObjects.Add(new DBObject(item, colections));
        //        }
        //        return dBObjects;
        //    }
        //    else
        //    {
        //        return new List<DBObject> { };
        //    }
        //}
        public async Task<Response> GetColectionsAsync(string databasename)
        {
            var database = Client.GetDatabase(databasename);
            var cursor = await database.ListCollectionNamesAsync();
            List<string> list =  await cursor.ToListAsync();
            if (list.Count is 0) return new Response(false, "Коллекции не найдены");
            else return new Response(true, list);
        }
        public async Task<Response> GetRecords(DocumentQueryRequest request)
        {
            var database = Client.GetDatabase(request.Database);
            var colection = database.GetCollection<BsonDocument>(request.Collection);
            var filter = BuildFilter(request.Filters);

            List<BsonDocument> documents = await colection
            .Find(filter)
            .Limit(request.Limit)
            .ToListAsync();
            if (documents.Count is 0) return new Response(false,"Документы не найдены");
            else return new Response(true,documents);

        }
        //public async Task<List<Field>> GetFields(DocumentQueryRequest request)
        //{
        //    var database = Client.GetDatabase(request.Database);
        //    var collection = database.GetCollection<BsonDocument>(request.Collection);

        //    // Берём первые 100 документов для статистики
        //    var documents = await collection.Find(FilterDefinition<BsonDocument>.Empty)
        //                                    .Limit(100)
        //                                    .ToListAsync();

        //    // Словарь: имя поля -> true, если поле может быть строкой
        //    var fieldIsString = new Dictionary<string, bool>();

        //    foreach (var document in documents)
        //    {
        //        foreach (var element in document.Elements)
        //        {
        //            string name = element.Name;

        //            // Проверяем, является ли значение строкой ИЛИ null (который тоже может быть строкой)
        //            bool isString = element.Value.BsonType == BsonType.String ||
        //                        element.Value.BsonType == BsonType.Null;

        //            if (!fieldIsString.ContainsKey(name))
        //                fieldIsString[name] = isString;
        //            else
        //                fieldIsString[name] &= isString;
        //        }
        //    }

        //    return fieldIsString.Select(kvp => new Field
        //    {
        //        Name = kvp.Key,
        //        IsString = kvp.Value
        //    })
        //    .OrderBy(f => f.Name)
        //    .ToList();
        //}
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
        //public async Task<UpdateResult> Update(string databasename, string collectionname, JsonElement filterel, List<Change> changes)
        //{
        //    var database = Client.GetDatabase(databasename);
        //    var collection = database.GetCollection<BsonDocument>(collectionname);

        //    // Конвертируем JsonElement в строку JSON
        //    var filterJson = filterel.GetRawText();
        //    BsonDocument filter = BsonDocument.Parse(filterJson);


        //    var updateBuilder = Builders<BsonDocument>.Update;

        //    //var convertedChanges = new List<Change>();
        //    //foreach (var change in changes)
        //    //{
        //    //    var convertedChange = new Change
        //    //    {
        //    //        Path = change.Path,
        //    //        Value = ConvertJsonElementToBsonValue(change.Value)
        //    //    };
        //    //    convertedChanges.Add(convertedChange);
        //    //}

        //    var updates = new List<UpdateDefinition<BsonDocument>>();

        //    foreach (var change in changes)
        //    {
        //        // Используем BsonValue.Create для автоматической конвертации
        //        updates.Add(updateBuilder.Set(change.Path, ConvertJsonElementToBsonValue(change.Value)));
        //    }
        //    UpdateDefinition<BsonDocument> update = updateBuilder.Combine(updates);

        //    return await collection.UpdateOneAsync(filter, update);
        //}
        //private BsonValue ConvertJsonElementToBsonValue(JsonElement element)
        //{
        //    return element.ValueKind switch
        //    {
        //        JsonValueKind.String => new BsonString(element.GetString()),
        //        JsonValueKind.Number => element.TryGetInt32(out int intVal)
        //            ? new BsonInt32(intVal)
        //            : new BsonDouble(element.GetDouble()),
        //        JsonValueKind.True => new BsonBoolean(true),
        //        JsonValueKind.False => new BsonBoolean(false),
        //        JsonValueKind.Null => BsonNull.Value,
        //        JsonValueKind.Object => BsonDocument.Parse(element.GetRawText()),
        //        _ => new BsonString(element.GetRawText())
        //    };
        //}




        public async Task<Response> CreateSchemaAsync(Schema schema)
        {
            await Schemas.InsertOneAsync(schema);
            return new Response(true, schema.Id);
        }

        public async Task<Response> GetSchemaAsync(string id)
        {
            Schema? schema = await Schemas
                .Find(x => x.Id == id)
                .FirstOrDefaultAsync();
            if (schema is not null) return new Response(true, schema);
            else return new Response(false, "Схема не найдена");
        }

        public async Task<Response> GetAllSchemasAsync()
        {
            List<Schema> list = await Schemas
                .Find(_ => true)
                .ToListAsync();
            if (list.Count > 0) return new Response(true, list);
            else return new Response(false, "Коллекция пуста.");
        }


        public async Task<Response> CreateDatabase(string name)
        {
            Client.GetDatabase(name);
            return new Response(true, name);
        }

        public async Task<Response> CreateCollectionInfoAsync(CollectionInfo collection)
        {
            await Collections.InsertOneAsync(collection);
            await System.CreateCollectionAsync(collection.Name);
            return new Response(true, collection.Id);
        }

        public async Task<Response> GetCollectionInfoByNameAsync(string name)
        {
            CollectionInfo? collectionInfo = await Collections
                .Find(x => x.Name == name)
                .FirstOrDefaultAsync();
            if (collectionInfo is not null) return new Response(true, collectionInfo);
            else return new Response(false, "Коллекция не найдена");
        }


        public async Task<Response> GetFieldsAsync(string collectionName)
        {
            // 1. Получить описание коллекции
            var collectionInfo = await GetCollectionInfoByNameAsync(collectionName);

            // 2. Получить схему
            if (collectionInfo.Message is not CollectionInfo _collectionInfo) return collectionInfo;
            var schema = await GetSchemaAsync(_collectionInfo.SchemaId);

            // 3. Проверить документ
            if (schema.Message is not Schema _schema) return schema;
            return new Response(true, _schema.Fields);
        }

        public async Task<Response> ReplaceDocumentAsync(RequestDocument request)
        {
            var document = request.Document.ToBsonDocument();

            // Получить информацию о коллекции
            var collectionResponse = await GetCollectionInfoByNameAsync(request.CollectionName);

            if (collectionResponse.Message is not CollectionInfo collectionInfo)
                return collectionResponse;

            // Получить схему
            var schemaResponse = await GetSchemaAsync(collectionInfo.SchemaId);

            if (schemaResponse.Message is not Schema schema)
                return schemaResponse;

            var mongoDatabase = Client.GetDatabase(request.DatabaseName);

            var mongoCollection = mongoDatabase.GetCollection<BsonDocument>(request.CollectionName);

            // 1. Проверка обязательных полей и типов
            await ValidateDocumentAsync(schema, document);

            //// 2. Автоматические действия
            //await ApplyActionsAsync(
            //    collectionInfo,
            //    schema,
            //    mongoCollection,
            //    document);

            // 3. Проверка уникальности
            await ValidateUniqueAsync(
                schema,
                mongoCollection,
                document);

            // 4. Служебные поля
            document["_updatedAt"] = DateTime.UtcNow;

            // 5. Сохранить
            ReplaceOneResult result =  await mongoCollection.ReplaceOneAsync(d => d == document["_id"],document);
            if (result.ModifiedCount > 0)
            {
                HistoryInfo historyInfo = new HistoryInfo { DatabaseName =request.DatabaseName,CollectionName = request.CollectionName, Data = document, IdDocument = document["_id"].AsString };
                await History.InsertOneAsync(historyInfo);
                return new Response(true,historyInfo.Id);
            }
            else return new Response(false, "Документ не изменен");
        }

        public async Task<Response> InsertDocumentAsync(RequestDocument request)
        {
            var document = request.Document.ToBsonDocument();

            // Получить информацию о коллекции
            var collectionResponse = await GetCollectionInfoByNameAsync(request.CollectionName);

            if (collectionResponse.Message is not CollectionInfo collectionInfo)
                return collectionResponse;

            // Получить схему
            var schemaResponse = await GetSchemaAsync(collectionInfo.SchemaId);

            if (schemaResponse.Message is not Schema schema)
                return schemaResponse;

            var mongoDatabase = Client.GetDatabase(request.DatabaseName);

            var mongoCollection = mongoDatabase.GetCollection<BsonDocument>(request.CollectionName);


            // 1. Проверка обязательных полей и типов
            await ValidateDocumentAsync(schema, document);

            // 2. Автоматические действия
            await ApplyActionsAsync(
                collectionInfo,
                schema,
                mongoCollection,
                document);

            // 3. Проверка уникальности
            await ValidateUniqueAsync(
                schema,
                mongoCollection,
                document);

            // 4. Служебные поля
            document["_createdAt"] = DateTime.UtcNow;
            document["_updatedAt"] = DateTime.UtcNow;

            // 5. Сохранить
            await mongoCollection.InsertOneAsync(document);

            return new Response(true, document["_id"]);
        }



        private async Task ValidateDocumentAsync(Schema schema, BsonDocument document)
        {
            //var schema = await GetSchemaAsync(collectionInfo.SchemaId);

            //// 3. Проверить документ
            //if (schema.Message is not Schema _schema) throw new Exception(schema.Message.ToString());

            foreach (var field in schema.Fields)
            {
                if (!document.Contains(field.Name))
                {
                    if (field.Required && !document.Contains(field.Name))
                        throw new Exception($"Поле '{field.Name}' обязательно для заполнения.");

                    continue;

                }
                var value = document[field.Name];

                switch (field.Type)
                {
                    case FieldType.String:

                        if (!value.IsString)
                            throw new Exception($"{field.Name} должен быть строкой.");

                        break;

                    case FieldType.Number:

                        if (!value.IsInt32 &&
                            !value.IsInt64 &&
                            !value.IsDouble &&
                            !value.IsDecimal128)
                            throw new Exception($"{field.Name} должен быть номером.");

                        break;

                    case FieldType.Boolean:

                        if (!value.IsBoolean)
                            throw new Exception($"{field.Name} должен быть логическим значением.");

                        break;

                    case FieldType.DateTime:

                        if (!value.IsValidDateTime)
                            throw new Exception($"{field.Name} должен быть датой.");

                        break;
                }


            }
        }
        private async Task ApplyActionsAsync(CollectionInfo collection,Schema schema,IMongoCollection<BsonDocument> mongoCollection,BsonDocument document)
        {
            var counters = System.GetCollection<BsonDocument>("Counters");

            foreach (var field in schema.Fields)
            {
                if (!field.AutoIncrement)
                    continue;

                var update =
                    Builders<BsonDocument>.Update.Inc(
                        $"fields.{field.Name}", 1);

                var options = new FindOneAndUpdateOptions<BsonDocument>
                {
                    IsUpsert = true,
                    ReturnDocument = ReturnDocument.After
                };

                var counter =
                    await counters.FindOneAndUpdateAsync(
                        Builders<BsonDocument>.Filter.Eq("_id", collection.Id),
                        update,
                        options);

                document[field.Name] =
                    counter["fields"][field.Name].AsInt64;
            }
        }
        private async Task ValidateUniqueAsync(Schema schema,IMongoCollection<BsonDocument> mongoCollection,BsonDocument document)
        {
            foreach (var field in schema.Fields)
            {
                if (!field.Unique)
                    continue;

                if (!document.Contains(field.Name))
                    continue;

                var value = document[field.Name];

                var filter =
                    Builders<BsonDocument>.Filter.Eq(field.Name, value);

                var exists =
                    await mongoCollection.Find(filter).AnyAsync();

                if (exists)
                    throw new Exception(
                        $"'{field.Name}' должен быть уникальным.");
            }
        }
    }
}
