using System.Net.Http.Json;
using NewsletterService.Models;

namespace NewsletterService.Services;

public class ArticleServiceClient
{
    private readonly HttpClient _httpClient;

    public ArticleServiceClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ArticleResponse?> GetLatestArticleAsync(
        string region,
        CancellationToken cancellationToken = default)
    {
        var articles =
            await _httpClient.GetFromJsonAsync<List<ArticleResponse>>(
                $"api/articles/{region}",
                cancellationToken);

        return articles?
            .OrderByDescending(a => a.PublishedAt)
            .FirstOrDefault();
    }
}