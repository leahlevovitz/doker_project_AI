using MongoDB.Driver;
using ProductCatalogService.Cache;
using Serilog;
using StackExchange.Redis;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Service", "ProductCatalogService")
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .WriteTo.Seq(Environment.GetEnvironmentVariable("Seq__ServerUrl") ?? "http://localhost:5341")
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();

var mongoConnectionString = builder.Configuration.GetValue<string>("MongoDB:ConnectionString") ?? "mongodb://localhost:27017";
var mongoDatabaseName = builder.Configuration.GetValue<string>("MongoDB:DatabaseName") ?? "ChineseAuction";
var redisConnection = builder.Configuration.GetValue<string>("Redis:ConnectionString") ?? "localhost:6379";

builder.Services.AddSingleton<IMongoClient>(_ => new MongoClient(mongoConnectionString));
builder.Services.AddSingleton(sp =>
    sp.GetRequiredService<IMongoClient>().GetDatabase(mongoDatabaseName));
builder.Services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisConnection));
builder.Services.AddSingleton<GiftCacheService>();

builder.Services.AddHealthChecks();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.Use(async (ctx, next) =>
{
    ctx.Response.Headers["X-Container-Id"] = Environment.MachineName;
    await next();
});

app.UseSwagger();
app.UseSwaggerUI();
app.MapHealthChecks("/health");
app.MapControllers();
app.Run();
