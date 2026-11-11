namespace NewsletterService.Services;

public interface IFeatureToggle
{
    Task<bool> IsSubscriberServiceEnabledAsync();
}