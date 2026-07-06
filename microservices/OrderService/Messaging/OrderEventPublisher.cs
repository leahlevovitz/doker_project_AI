using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using Shared.Events;

namespace OrderService.Messaging;

public class OrderEventPublisher : IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private const string Exchange = "saga";

    public OrderEventPublisher(RabbitMqConnectionFactory factory)
    {
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        _channel.ExchangeDeclare(Exchange, ExchangeType.Topic, durable: true);
    }

    public void Publish<T>(T @event, string routingKey)
    {
        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(@event));
        var props = _channel.CreateBasicProperties();
        props.Persistent = true;
        props.MessageId = Guid.NewGuid().ToString();
        _channel.BasicPublish(Exchange, routingKey, props, body);
    }

    public void Dispose() { _channel.Dispose(); _connection.Dispose(); }
}
