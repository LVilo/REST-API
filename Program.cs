
using MongoAPI.Services;
using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

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
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters =
                        new TokenValidationParameters
                        {
                            ValidateIssuer = true,
                            ValidateAudience = true,
                            ValidateLifetime = true,
                            ValidateIssuerSigningKey = true,

                            ValidIssuer = "MongoAPI",
                            ValidAudience = "MongoAPI",

                            IssuerSigningKey =
                                new SymmetricSecurityKey(
                                    Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
                        };
                });

            builder.Services.AddAuthorization();
            var connectionString = builder.Configuration["Database:ConnectionString"];
            Database database = new Mongo(connectionString);
            builder.Services.AddScoped<JwtService>();
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
            //if (app.Environment.IsDevelopment())
            //{
                app.UseSwagger();
                app.UseSwaggerUI();
            //}
        //app.UseHttpsRedirection();

            app.UseCors(policy => policy
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());

            app.UseAuthorization();
            app.MapControllers();
            app.Run();
        }
    }
}
