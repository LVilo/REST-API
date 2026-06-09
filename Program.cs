
using MongoAPI.Services;
using Asp.Versioning;

namespace MongoAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1);

                // если версия не указана -> использовать v1
                options.AssumeDefaultVersionWhenUnspecified = true;

                // добавляет информацию о версиях в headers
                options.ReportApiVersions = true;

                // версия в URL
                options.ApiVersionReader = new UrlSegmentApiVersionReader();
            });

            var connectionString = builder.Configuration["Database:ConnectionString"];
            Database database = new Mongo(connectionString);

            builder.Services.AddSingleton(database);


            //builder.Services.Configure<Database>(builder.Configuration.GetSection("Database"));

            //var Database = builder.Configuration.GetSection("Database").Get<Database>();

            //builder.Services.AddSingleton<Database>(
            //    new Mongo(Database.ConnectionString));


            // Добавляем контроллеры
            builder.Services.AddControllers();

            // Добавляем Swagger
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Включаем Swagger
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
        //app.UseHttpsRedirection();

        app.UseAuthorization();
            app.MapControllers();
            app.Run();
        }
    }
}
