using System.Diagnostics;
using System.Text;
using System.Text.Json;
using NewsletterService.Models;
using OpenTelemetry.Context.Propagation;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace NewsletterService.Services;

public class NewsletterArticleConsumer : BackgroundService
{
    private static readonly ActivitySource ActivitySource =
        new("NewsletterService");

    private static readonly TextMapPropagator Propagator =
        Propagators.DefaultTextMapPropagator;

    private readonly IConfiguration _configuration;

    private IConnection? _connection;
    private IChannel? _channel;

    public NewsletterArticleConsumer(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = _configuration["RabbitMQ:Host"] ?? "localhost"
        };

        _connection = await factory.CreateConnectionAsync(stoppingToken);

        _channel = await _connection.CreateChannelAsync(
            cancellationToken: stoppingToken);

        await _channel.ExchangeDeclareAsync(
            exchange: "articles",
            type: ExchangeType.Fanout,
            durable: true,
            autoDelete: false,
            cancellationToken: stoppingToken);

        await _channel.QueueDeclareAsync(
            queue: "newsletter-articles",
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: stoppingToken);

        await _channel.QueueBindAsync(
            queue: "newsletter-articles",
            exchange: "articles",
            routingKey: "",
            cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(_channel);

        consumer.ReceivedAsync += async (_, ea) =>
        {
            var parentContext = Propagator.Extract(
                default,
                ea.BasicProperties.Headers,
                (headers, key) =>
                {
                    if (headers != null &&
                        headers.TryGetValue(key, out var value) &&
                        value is byte[] bytes)
                    {
                        return new[]
                        {
                            Encoding.UTF8.GetString(bytes)
                        };
                    }

                    return Array.Empty<string>();
                });

            using var activity = ActivitySource.StartActivity(
                "newsletter article consume",
                ActivityKind.Consumer,
                parentContext.ActivityContext);

            try
            {
                var json = Encoding.UTF8.GetString(
                    ea.Body.ToArray());

                var article =
                    JsonSerializer.Deserialize<PublishedArticleMessage>(json);

                if (article == null)
                {
                    throw new InvalidOperationException(
                        "Could not deserialize published article.");
                }

                Console.WriteLine(
                    $"Newsletter received: {article.Title} - " +
                    $"TraceId: {activity?.TraceId}");

                await _channel.BasicAckAsync(
                    ea.DeliveryTag,
                    multiple: false,
                    cancellationToken: stoppingToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"Failed to consume newsletter article: {ex.Message}");

                await _channel.BasicNackAsync(
                    ea.DeliveryTag,
                    multiple: false,
                    requeue: false,
                    cancellationToken: stoppingToken);
            }
        };

        await _channel.BasicConsumeAsync(
            queue: "newsletter-articles",
            autoAck: false,
            consumer: consumer,
            cancellationToken: stoppingToken);
    }

    public override async Task StopAsync(
        CancellationToken cancellationToken)
    {
        if (_channel != null)
        {
            await _channel.CloseAsync(cancellationToken);
        }

        if (_connection != null)
        {
            await _connection.CloseAsync(cancellationToken);
        }

        await base.StopAsync(cancellationToken);
    }
}