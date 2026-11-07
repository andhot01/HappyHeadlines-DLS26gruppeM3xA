using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using SubscriberService.Models;

namespace SubscriberService.Services;

public class SubscriberQueuePublisher
{
    private readonly ConnectionFactory _factory;

    public SubscriberQueuePublisher(IConfiguration configuration)
    {
        _factory = new ConnectionFactory
        {
            HostName = configuration["RabbitMQ:Host"] ?? "localhost"
        };
    }

    public async Task PublishAsync(Subscriber subscriber)
    {
        await using var connection = await _factory.CreateConnectionAsync();
        await using var channel = await connection.CreateChannelAsync();

        await channel.QueueDeclareAsync(
            queue: "subscribers",
            durable: true,
            exclusive: false,
            autoDelete: false);
        
        var json = JsonSerializer.Serialize(subscriber);
        var body = Encoding.UTF8.GetBytes(json);

        var properties = new BasicProperties
        {
            DeliveryMode = DeliveryModes.Persistent
        };

        await channel.BasicPublishAsync(
            exchange: "",
            routingKey: "subscribers",
            mandatory: false,
            basicProperties: properties,
            body: body);
    }
}