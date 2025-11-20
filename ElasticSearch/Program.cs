using Elastic.Clients.Elasticsearch;
using ElasticSearch.Configuration;
using ElasticSearch.Handlers;
using ElasticSearch.Services;
using Microsoft.Extensions.Options;
using Serilog;



namespace ElasticSearch
{
    public class Program()
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            
            var configuration = builder.Configuration;
            LoggingConfig.ConfigureLogging(configuration);
            builder.Host.UseSerilog(Log.Logger);

            ElasticSettings elasticSettings = configuration.GetSection("ElasticsSearchSettings").Get<ElasticSettings>() ?? throw new Exception("ElasticsSearchSettings configuration not found");

            var settings = new ElasticsearchClientSettings(new Uri(elasticSettings.Url))
                           //.Authentication()
                           .DefaultIndex(elasticSettings.DefaultIndex);

            // Add services to the container.
            builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
            builder.Services.AddProblemDetails();
            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddSingleton(new ElasticsearchClient(settings));
            builder.Services.AddScoped(typeof(IElasticService<>), typeof(ElasticService<>));
            builder.Services.AddScoped<IProductService, ProductService>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseExceptionHandler();
            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}

