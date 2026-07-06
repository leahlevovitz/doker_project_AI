using Microsoft.EntityFrameworkCore;
using OrderService.Data;
using OrderService.Messaging;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Server=sqlserver;Database=OrdersDb;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True";

var rabbitHost = builder.Configuration.GetValue<string>("RabbitMQ:Host") ?? "rabbitmq";

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddSingleton(new RabbitMqConnectionFactory(rabbitHost));
builder.Services.AddSingleton<OrderEventPublisher>();
builder.Services.AddHostedService<InventoryResponseConsumer>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var retries = 10;
    while (retries-- > 0)
    {
        try { db.Database.Migrate(); break; }
        catch { Thread.Sleep(3000); }
    }
}

app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();
app.Run();
