
using MongoAPI.Services;

namespace MongoAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Добавляем MongoDeviceService как singleton
            builder.Services.AddSingleton(new MongoDeviceService("mongodb://localhost:27017"));
            //builder.Services.AddSingleton(new MongoDeviceService("mongodb://localhost:27017"));

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

            app.UseAuthorization();
            app.MapControllers();
            app.Run();
        }
    }
}
