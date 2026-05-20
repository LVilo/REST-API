
using MongoAPI.Services;

namespace MongoAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.WebHost.ConfigureKestrel(options =>
            {
                options.ListenAnyIP(80);

                options.ListenAnyIP(443, listenOptions =>
                {
                    listenOptions.UseHttps("cert.pfx", "password");
                });
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
            app.UseHttpsRedirection();

            app.UseAuthorization();
            app.MapControllers();
            app.Run();
        }
    }
}
