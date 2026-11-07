using System.Text;
using System.Text.Json;
using NewsletterService.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace NewsletterService.Services;

public class SubscriberConsumer : BackgroundService
{
    private readonly IConfiguration _configuration;

    private IConnection? _connection;
    private IChannel? _channel;

    public SubscriberConsumer(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = _configuration["RabbitMQ:Host"] ?? "localhost"
        };

        _connection = await factory.CreateConnectionAsync(stoppingToken);

        _channel = await _connection.CreateChannelAsync(
            cancellationToken: stoppingToken);

        await _channel.QueueDeclareAsync(
            queue: "subscribers",
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(_channel);

        consumer.ReceivedAsync += async (_, ea) =>
        {
            try
            {
                var json = Encoding.UTF8.GetString(ea.Body.ToArray());

                var subscriber =
                    JsonSerializer.Deserialize<SubscriberMessage>(json);

                if (subscriber == null)
                {
                    throw new InvalidOperationException(
                        "Could not deserialize subscriber.");
                }

                Console.WriteLine(
                    $"Newsletter received new subscriber: {subscriber.Email}");

                await _channel.BasicAckAsync(
                    ea.DeliveryTag,
                    multiple: false,
                    cancellationToken: stoppingToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"Failed to consume subscriber: {ex.Message}");

                await _channel.BasicNackAsync(
                    ea.DeliveryTag,
                    multiple: false,
                    requeue: false,
                    cancellationToken: stoppingToken);
            }
        };

        await _channel.BasicConsumeAsync(
            queue: "subscribers",
            autoAck: false,
            consumer: consumer,
            cancellationToken: stoppingToken);
    }

    public override async Task StopAsync(
        CancellationToken cancellationToken)
    {
        if (_channel != null)
            await _channel.CloseAsync(cancellationToken);

        if (_connection != null)
            await _connection.CloseAsync(cancellationToken);

        await base.StopAsync(cancellationToken);
    }
}