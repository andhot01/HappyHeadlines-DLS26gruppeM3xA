using System.Diagnostics;
using System.Text;
using System.Text.Json;
using ArticleService.Models;
using ArticleService.Repositories;
using OpenTelemetry.Context.Propagation;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ArticleService.Services;

public class ArticleQueueConsumer : BackgroundService
{
    private static readonly ActivitySource ActivitySource =
        new("ArticleService");

    private static readonly TextMapPropagator Propagator =
        Propagators.DefaultTextMapPropagator;

    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;

    private IConnection? _connection;
    private IChannel? _channel;

    public ArticleQueueConsumer(
        IServiceProvider serviceProvider,
        IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
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

        await _channel.QueueDeclareAsync(
            queue: "articles",
            durable: true,
            exclusive: false,
            autoDelete: false,
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
                        return new[] { Encoding.UTF8.GetString(bytes) };
                    }

                    return Array.Empty<string>();
                });

            using var activity = ActivitySource.StartActivity(
                "articles consume",
                ActivityKind.Consumer,
                parentContext.ActivityContext);

            try
            {
                var json = Encoding.UTF8.GetString(ea.Body.ToArray());

                var message = JsonSerializer.Deserialize<PublishedArticleMessage>(
                    json);

                if (message == null)
                {
                    throw new InvalidOperationException(
                        "Could not deserialize published article.");
                }

                if (!Enum.TryParse<Region>(
                        message.Region,
                        ignoreCase: true,
                        out var region))
                {
                    throw new InvalidOperationException(
                        $"Unknown article region: {message.Region}");
                }

                var article = new Article
                {
                    Id = message.Id,
                    Title = message.Title,
                    Content = message.Content,
                    Region = region,
                    PublishedAt = message.PublishedAt
                };

                using var scope = _serviceProvider.CreateScope();

                var repository =
                    scope.ServiceProvider.GetRequiredService<IArticleRepository>();

                repository.Create(article);

                Console.WriteLine(
                    $"Article consumed: {article.Title} - TraceId: {activity?.TraceId}");

                await _channel.BasicAckAsync(
                    ea.DeliveryTag,
                    multiple: false,
                    cancellationToken: stoppingToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"Failed to consume article: {ex.Message}");

                await _channel.BasicNackAsync(
                    ea.DeliveryTag,
                    multiple: false,
                    requeue: false,
                    cancellationToken: stoppingToken);
            }
        };

        await _channel.BasicConsumeAsync(
            queue: "articles",
            autoAck: false,
            consumer: consumer,
            cancellationToken: stoppingToken);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
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

public class PublishedArticleMessage
{
    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public string Region { get; set; } = string.Empty;

    public DateTime PublishedAt { get; set; }
}