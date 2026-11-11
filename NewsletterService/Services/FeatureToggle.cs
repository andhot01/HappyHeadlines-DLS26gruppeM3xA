using FeatureHubSDK;

namespace NewsletterService.Services;

public class FeatureHubToggle : IFeatureToggle
{
    private readonly IFeatureHubConfig _config;
    private readonly ILogger<FeatureHubToggle> _logger;

    public FeatureHubToggle(
        IFeatureHubConfig config,
        ILogger<FeatureHubToggle> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task<bool> IsSubscriberServiceEnabledAsync()
    {
        try
        {
            var context = await _config.NewContext().Build();
            return context["SubscriberServiceEnabled"].IsEnabled;
        }
        catch (Exception ex)
        {
            // Fail closed: hvis FeatureHub ikke kan nås, behandles featuren som slukket
            _logger.LogWarning(ex, "Could not read feature toggle, treating as disabled.");
            return false;
        }
    }
}