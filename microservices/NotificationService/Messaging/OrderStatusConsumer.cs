using System.Text;
using System.Text.Json;
using MongoDB.Driver;
using NotificationService.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared.Events;

namespace NotificationService.Messaging;

public class OrderStatusConsumer : BackgroundService
{
    private readonly ILogger<OrderStatusConsumer> _logger;
    private readonly IMongoCollection<LotteryResult> _notifications;
    private IConnection? _connection;
    private IModel? _channel;
    private const string Exchange = "saga";

    public OrderStatusConsumer(
        ILogger<OrderStatusConsumer> logger,
        IMongoDatabase db,
        RabbitMqConnectionFactory factory)
    {
        _logger = logger;
        _notifications = db.GetCollection<LotteryResult>("notifications");
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        _channel.ExchangeDeclare(Exchange, ExchangeType.Topic, durable: true);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var confirmedQueue = "notification.order.confirmed";
        _channel!.QueueDeclare(confirmedQueue, durable: true, exclusive: false, autoDelete: false);
        _channel.QueueBind(confirmedQueue, Exchange, "order.confirmed");

        var cancelledQueue = "notification.order.cancelled";
        _channel.QueueDeclare(cancelledQueue, durable: true, exclusive: false, autoDelete: false);
        _channel.QueueBind(cancelledQueue, Exchange, "order.cancelled");

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.Received += async (_, ea) =>
        {
            var body = Encoding.UTF8.GetString(ea.Body.ToArray());
            var routingKey = ea.RoutingKey;
            try
            {
                if (routingKey == "order.confirmed")
                {
                    var evt = JsonSerializer.Deserialize<OrderConfirmedEvent>(body)!;
                    _logger.LogInformation(
                        "[NotificationService] ✅ ORDER CONFIRMED — PurchaseId={Id} GiftId={GiftId} UserId={UserId} CorrelationId={CorrelationId}. Notifying user.",
                        evt.PurchaseId, evt.GiftId, evt.UserId, evt.CorrelationId);
                }
                else if (routingKey == "order.cancelled")
                {
                    var evt = JsonSerializer.Deserialize<OrderCancelledEvent>(body)!;
                    _logger.LogWarning(
                        "[NotificationService] ❌ ORDER CANCELLED — PurchaseId={Id} GiftId={GiftId} UserId={UserId} Reason={Reason} CorrelationId={CorrelationId}. Notifying user.",
                        evt.PurchaseId, evt.GiftId, evt.UserId, evt.Reason, evt.CorrelationId);
                }

                _channel.BasicAck(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[NotificationService] Error processing order status event");
                _channel.BasicNack(ea.DeliveryTag, false, requeue: true);
            }
            await Task.CompletedTask;
        };

        _channel.BasicConsume(confirmedQueue, autoAck: false, consumer: consumer);
        _channel.BasicConsume(cancelledQueue, autoAck: false, consumer: consumer);
        return Task.CompletedTask;
    }

    public override void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
        base.Dispose();
    }
}
