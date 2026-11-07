using NewsletterService.Models;

namespace NewsletterService.Services;

public class SubscriberServiceClient
{
    private readonly HttpClient _httpClient;
    
    public SubscriberServiceClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<SubscriberMessage>> GetSubscriberAsync(
        CancellationToken cancellationToken = default)
    {
        return await _httpClient.GetFromJsonAsync<List<SubscriberMessage>>(
            "api/subscribers",
            cancellationToken) ?? new List<SubscriberMessage>();
    }
}