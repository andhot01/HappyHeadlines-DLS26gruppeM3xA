using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Trace;

namespace Observability;

public static class TracingExtensions
{
    public static IServiceCollection AddHappyHeadlinesTracing(
        this IServiceCollection services,
        string serviceName)
    {
        services.AddOpenTelemetry()
            .WithTracing(tracing =>
            {
                tracing
                    .AddSource(serviceName)
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddConsoleExporter();
            });

        return services;
    }
}