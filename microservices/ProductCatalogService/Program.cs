using MongoDB.Driver;
using ProductCatalogService.Cache;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

var mongoConnectionString = builder.Configuration.GetValue<string>("MongoDB:ConnectionString") ?? "mongodb://localhost:27017";
var mongoDatabaseName = builder.Configuration.GetValue<string>("MongoDB:DatabaseName") ?? "ChineseAuction";
var redisConnection = builder.Configuration.GetValue<string>("Redis:ConnectionString") ?? "localhost:6379";

builder.Services.AddSingleton<IMongoClient>(_ => new MongoClient(mongoConnectionString));
builder.Services.AddSingleton(sp =>
    sp.GetRequiredService<IMongoClient>().GetDatabase(mongoDatabaseName));
builder.Services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisConnection));
builder.Services.AddSingleton<GiftCacheService>();

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
app.MapControllers();
app.Run();
