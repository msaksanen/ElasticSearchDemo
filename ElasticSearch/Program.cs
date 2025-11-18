using ElasticSearch.Configuration;
using ElasticSearch.Services;
using Serilog;



namespace ElasticSearch
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.Configure<ElasticSettings>(builder.Configuration.GetSection("ElasticsSearchSettings"));
            var _elasticURL = builder.Configuration["ElasticsSearchSettings:Url"];
            LoggingConfig.ConfigureLogging(_elasticURL);
            builder.Host.UseSerilog(Log.Logger);

            // Add services to the container.
            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
          
            builder.Services.AddSingleton(typeof(IElasticService<>), typeof(ElasticService<>));


            var app = builder.Build();

            // Configure the HTTP request pipeline.
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

