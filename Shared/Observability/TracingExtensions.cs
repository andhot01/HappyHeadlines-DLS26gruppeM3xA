using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Trace;
using OpenTelemetry.Resources;

namespace Observability;

public static class TracingExtensions
{
    public static IServiceCollection AddHappyHeadlinesTracing(
        this IServiceCollection services, string serviceName)
    {
        services.AddOpenTelemetry().ConfigureResource(resource => resource.AddService(serviceName))
            .WithTracing(tracing =>
            {
                tracing
                    .AddSource(serviceName)
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddConsoleExporter()
                    .AddOtlpExporter();
            });

        return services;
    }
}