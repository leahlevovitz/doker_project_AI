using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using OrderService.Data;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared.Events;

namespace OrderService.Messaging;

public class InventoryResponseConsumer : BackgroundService
{
    private readonly ILogger<InventoryResponseConsumer> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly OrderEventPublisher _publisher;
    private IConnection? _connection;
    private IModel? _channel;
    private const string Exchange = "saga";

    public InventoryResponseConsumer(
        ILogger<InventoryResponseConsumer> logger,
        IServiceScopeFactory scopeFactory,
        OrderEventPublisher publisher,
        RabbitMqConnectionFactory factory)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
        _publisher = publisher;
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        _channel.ExchangeDeclare(Exchange, ExchangeType.Topic, durable: true);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Queue for InventoryReserved
        var reservedQueue = "order.inventory.reserved";
        _channel!.QueueDeclare(reservedQueue, durable: true, exclusive: false, autoDelete: false);
        _channel.QueueBind(reservedQueue, Exchange, "inventory.reserved");

        // Queue for InventoryRejected
        var rejectedQueue = "order.inventory.rejected";
        _channel.QueueDeclare(rejectedQueue, durable: true, exclusive: false, autoDelete: false);
        _channel.QueueBind(rejectedQueue, Exchange, "inventory.rejected");

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.Received += async (_, ea) =>
        {
            var body = Encoding.UTF8.GetString(ea.Body.ToArray());
            var routingKey = ea.RoutingKey;

            try
            {
                if (routingKey == "inventory.reserved")
                {
                    var evt = JsonSerializer.Deserialize<InventoryReservedEvent>(body)!;
                    using var scope = _scopeFactory.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    var purchase = await db.Purchases.FindAsync(evt.PurchaseId);
                    if (purchase == null || purchase.Status != "Pending")
                    {
                        _logger.LogWarning("[OrderService] Skipping duplicate InventoryReserved for PurchaseId={Id}", evt.PurchaseId);
                        _channel.BasicAck(ea.DeliveryTag, false);
                        return;
                    }
                    purchase.Status = "Confirmed";
                    await db.SaveChangesAsync();
                    _logger.LogInformation("[OrderService] Order CONFIRMED PurchaseId={Id} GiftId={GiftId} CorrelationId={CorrelationId}",
                        evt.PurchaseId, evt.GiftId, evt.CorrelationId);
                    _publisher.Publish(new OrderConfirmedEvent(Guid.NewGuid(), evt.CorrelationId, evt.PurchaseId, evt.GiftId, evt.UserId), "order.confirmed");
                }
                else if (routingKey == "inventory.rejected")
                {
                    var evt = JsonSerializer.Deserialize<InventoryRejectedEvent>(body)!;
                    using var scope = _scopeFactory.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    var purchase = await db.Purchases.FindAsync(evt.PurchaseId);
                    if (purchase == null || purchase.Status != "Pending")
                    {
                        _logger.LogWarning("[OrderService] Skipping duplicate InventoryRejected for PurchaseId={Id}", evt.PurchaseId);
                        _channel.BasicAck(ea.DeliveryTag, false);
                        return;
                    }
                    purchase.Status = "Cancelled";
                    await db.SaveChangesAsync();
                    _logger.LogWarning("[OrderService] Order CANCELLED PurchaseId={Id} Reason={Reason} CorrelationId={CorrelationId}",
                        evt.PurchaseId, evt.Reason, evt.CorrelationId);
                    _publisher.Publish(new OrderCancelledEvent(Guid.NewGuid(), evt.CorrelationId, evt.PurchaseId, evt.GiftId, evt.UserId, evt.Reason), "order.cancelled");
                }

                _channel.BasicAck(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[OrderService] Error processing message");
                _channel.BasicNack(ea.DeliveryTag, false, requeue: true);
            }
        };

        _channel.BasicConsume(reservedQueue, autoAck: false, consumer: consumer);
        _channel.BasicConsume(rejectedQueue, autoAck: false, consumer: consumer);

        return Task.CompletedTask;
    }

    public override void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
        base.Dispose();
    }
}
