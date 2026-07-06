using InventoryService.Messaging;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

var redisConnection = builder.Configuration.GetValue<string>("Redis:ConnectionString") ?? "localhost:6379";
var rabbitHost = builder.Configuration.GetValue<string>("RabbitMQ:Host") ?? "rabbitmq";

builder.Services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisConnection));
builder.Services.AddSingleton(new RabbitMqConnectionFactory(rabbitHost));
builder.Services.AddHostedService<OrderPlacedConsumer>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();
app.Run();
