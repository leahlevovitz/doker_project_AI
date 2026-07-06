using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared.Events;
using StackExchange.Redis;

namespace InventoryService.Messaging;

public class OrderPlacedConsumer : BackgroundService
{
    private readonly ILogger<OrderPlacedConsumer> _logger;
    private readonly IDatabase _redis;
    private IConnection? _connection;
    private IModel? _channel;
    private const string Exchange = "saga";
    private const string IdempotencyPrefix = "processed:inventory:";

    public OrderPlacedConsumer(
        ILogger<OrderPlacedConsumer> logger,
        IConnectionMultiplexer redis,
        RabbitMqConnectionFactory factory)
    {
        _logger = logger;
        _redis = redis.GetDatabase();
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        _channel.ExchangeDeclare(Exchange, ExchangeType.Topic, durable: true);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var queue = "inventory.order.placed";
        _channel!.QueueDeclare(queue, durable: true, exclusive: false, autoDelete: false);
        _channel.QueueBind(queue, Exchange, "order.placed");
        _channel.BasicQos(0, 1, false);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.Received += async (_, ea) =>
        {
            var body = Encoding.UTF8.GetString(ea.Body.ToArray());
            try
            {
                var evt = JsonSerializer.Deserialize<OrderPlacedEvent>(body)!;

                // Idempotency check - skip if already processed
                var idempotencyKey = $"{IdempotencyPrefix}{evt.MessageId}";
                if (await _redis.StringGetAsync(idempotencyKey) == "1")
                {
                    _logger.LogWarning("[InventoryService] Duplicate message MessageId={MsgId}, skipping", evt.MessageId);
                    _channel.BasicAck(ea.DeliveryTag, false);
                    return;
                }

                _logger.LogInformation("[InventoryService] Received OrderPlaced PurchaseId={Id} GiftId={GiftId}", evt.PurchaseId, evt.GiftId);

                var key = $"gift:{evt.GiftId}:quantity";
                var newQty = await _redis.StringDecrementAsync(key);

                if (newQty < 0)
                {
                    // Rollback
                    await _redis.StringIncrementAsync(key);
                    _logger.LogWarning("[InventoryService] Out of stock GiftId={GiftId}, publishing InventoryRejected", evt.GiftId);
                    Publish(new InventoryRejectedEvent(Guid.NewGuid(), evt.PurchaseId, evt.GiftId, evt.UserId, "Out of stock"), "inventory.rejected");
                }
                else
                {
                    _logger.LogInformation("[InventoryService] Reserved GiftId={GiftId} remaining={Qty}, publishing InventoryReserved", evt.GiftId, newQty);
                    Publish(new InventoryReservedEvent(Guid.NewGuid(), evt.PurchaseId, evt.GiftId, evt.UserId), "inventory.reserved");
                }

                // Mark as processed (TTL 24h for idempotency)
                await _redis.StringSetAsync(idempotencyKey, "1", TimeSpan.FromHours(24));
                _channel.BasicAck(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[InventoryService] Error processing OrderPlaced");
                _channel.BasicNack(ea.DeliveryTag, false, requeue: true);
            }
        };

        _channel.BasicConsume(queue, autoAck: false, consumer: consumer);
        return Task.CompletedTask;
    }

    private void Publish<T>(T @event, string routingKey)
    {
        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(@event));
        var props = _channel!.CreateBasicProperties();
        props.Persistent = true;
        _channel.BasicPublish(Exchange, routingKey, props, body);
    }

    public override void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
        base.Dispose();
    }
}
