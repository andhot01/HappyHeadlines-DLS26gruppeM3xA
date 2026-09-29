using System.Text;
using System.Text.Json;
using PublisherService.Models;
using RabbitMQ.Client;
using System.Diagnostics;
using OpenTelemetry.Context.Propagation;

namespace PublisherService.Services;

public class ArticleQueuePublisher
{
    private readonly ConnectionFactory _factory;
    private static readonly ActivitySource ActivitySource = new("PublisherService");
    private static readonly TextMapPropagator Propagator = Propagators.DefaultTextMapPropagator;

    public ArticleQueuePublisher(IConfiguration configuration)
    {
        _factory = new ConnectionFactory
        {
            HostName = configuration["RabbitMQ:Host"] ?? "localhost"
        };
    }

    public async Task PublishAsync(PublishedArticle article)
    {
        await using var connection = await _factory.CreateConnectionAsync();
        await using var channel = await connection.CreateChannelAsync();

        await channel.ExchangeDeclareAsync(
            exchange: "articles",
            type: ExchangeType.Fanout,
            durable: true,
            autoDelete: false);

        var json = JsonSerializer.Serialize(article);
        var body = Encoding.UTF8.GetBytes(json);

        // Create RabbitMQ message properties
        var properties = new BasicProperties
        {
            Headers = new Dictionary<string, object?>()
        };

        // Get the trace of the current HTTP request
        var activity = Activity.Current;

        if (activity != null)
        {
            var propagationContext = new PropagationContext(
                activity.Context,
                default);

            // Put the OpenTelemetry trace context into
            // the RabbitMQ message headers
            Propagator.Inject(
                propagationContext,
                properties.Headers,
                (headers, key, value) =>
                {
                    headers[key] = Encoding.UTF8.GetBytes(value);
                });
        }

        await channel.BasicPublishAsync(
            exchange: "articles",
            routingKey: "",
            mandatory: false,
            basicProperties: properties,
            body: body);
    }
}