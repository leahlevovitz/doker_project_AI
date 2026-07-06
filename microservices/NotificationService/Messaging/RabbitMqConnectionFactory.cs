using RabbitMQ.Client;

namespace NotificationService.Messaging;

public class RabbitMqConnectionFactory
{
    private readonly string _host;

    public RabbitMqConnectionFactory(string host) => _host = host;

    public IConnection CreateConnection()
    {
        var factory = new ConnectionFactory { HostName = _host, DispatchConsumersAsync = true };
        var retries = 15;
        while (retries-- > 0)
        {
            try { return factory.CreateConnection(); }
            catch { Thread.Sleep(3000); }
        }
        throw new Exception($"Cannot connect to RabbitMQ at {_host}");
    }
}
