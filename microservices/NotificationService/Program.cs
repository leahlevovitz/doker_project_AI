using MongoDB.Driver;
using NotificationService.Messaging;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Service", "NotificationService")
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .WriteTo.Seq(Environment.GetEnvironmentVariable("Seq__ServerUrl") ?? "http://localhost:5341")
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();

var mongoConnectionString = builder.Configuration.GetValue<string>("MongoDB:ConnectionString") ?? "mongodb://localhost:27017";
var mongoDatabaseName = builder.Configuration.GetValue<string>("MongoDB:DatabaseName") ?? "ChineseAuction";
var rabbitHost = builder.Configuration.GetValue<string>("RabbitMQ:Host") ?? "rabbitmq";

builder.Services.AddSingleton<IMongoClient>(_ => new MongoClient(mongoConnectionString));
builder.Services.AddSingleton(sp =>
    sp.GetRequiredService<IMongoClient>().GetDatabase(mongoDatabaseName));
builder.Services.AddSingleton(new RabbitMqConnectionFactory(rabbitHost));
builder.Services.AddHostedService<OrderStatusConsumer>();

builder.Services.AddHealthChecks()
    .AddMongoDb(mongoConnectionString, name: "mongodb");

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.MapHealthChecks("/health");
app.MapControllers();
app.Run();
