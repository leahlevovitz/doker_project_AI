using InventoryService.Messaging;
using Serilog;
using Serilog.Events;
using StackExchange.Redis;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Service", "InventoryService")
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .WriteTo.Seq(Environment.GetEnvironmentVariable("Seq__ServerUrl") ?? "http://localhost:5341")
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();

var redisConnection = builder.Configuration.GetValue<string>("Redis:ConnectionString") ?? "localhost:6379";
var rabbitHost = builder.Configuration.GetValue<string>("RabbitMQ:Host") ?? "rabbitmq";

builder.Services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisConnection));
builder.Services.AddSingleton(new RabbitMqConnectionFactory(rabbitHost));
builder.Services.AddHostedService<OrderPlacedConsumer>();

builder.Services.AddHealthChecks();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.MapHealthChecks("/health");
app.MapControllers();
app.Run();
