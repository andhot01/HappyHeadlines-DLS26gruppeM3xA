using Microsoft.Extensions.Logging;

namespace Observability;

public static class LoggingExtensions
{
    public static ILoggingBuilder AddHappyHeadlinesLogging(
        this ILoggingBuilder logging)
    {
        logging.ClearProviders();
        logging.AddConsole();

        return logging;
    }
}