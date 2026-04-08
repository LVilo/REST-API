using Microsoft.OpenApi;
using RestAPI.Data;
using RestAPI.Repositories;
using RestAPI.Services;
using Microsoft.EntityFrameworkCore;

namespace RestAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            new OpenApiCallback();
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Device Configuration Log API",
                    Version = "v1"
                });
            });

            var storageProvider = builder.Configuration["StorageProvider"];

            if (storageProvider == "Sql")
            {
                builder.Services.AddDbContext<AppDbContext>(options =>
                    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

                builder.Services.AddScoped<IConfigurationRepository, SqlConfigurationRepository>();
            }
            else if (storageProvider == "Mongo")
            {
                builder.Services.AddSingleton<MongoDbContext>();
                builder.Services.AddScoped<IConfigurationRepository, MongoConfigurationRepository>();
            }
            else
            {
                throw new Exception("StorageProvider must be either 'Sql' or 'Mongo'");
            }

            var app = builder.Build();

            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}
