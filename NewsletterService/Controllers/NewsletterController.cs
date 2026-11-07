using Microsoft.AspNetCore.Mvc;
using NewsletterService.Services;

namespace NewsletterService.Controllers;

[ApiController]
[Route("api/newsletter")]
public class NewsletterController : ControllerBase
{
    private readonly ArticleServiceClient _articleServiceClient;
    private readonly SubscriberServiceClient _subscriberServiceClient;

    public NewsletterController(
        ArticleServiceClient articleServiceClient,
        SubscriberServiceClient subscriberServiceClient)
    {
        _articleServiceClient = articleServiceClient;
        _subscriberServiceClient = subscriberServiceClient;
    }

    [HttpGet("daily/{region}")]
    public async Task<IActionResult> GetDailyNewsletter(
        string region,
        CancellationToken cancellationToken)
    {
        var article =
            await _articleServiceClient.GetLatestArticleAsync(
                region,
                cancellationToken);

        if (article == null)
        {
            return NotFound(
                $"No articles found for region '{region}'.");
        }

        return Ok(new
        {
            Type = "Daily Newsletter",
            Article = article
        });
    }

    [HttpGet("subscribers")]
    public async Task<IActionResult> GetSubscribers(CancellationToken cancellationToken)
    {
        try
        {
            var subscribers = await _subscriberServiceClient.GetSubscriberAsync(cancellationToken);
           
            return Ok(subscribers);
        }
        catch (HttpRequestException)
        {
            return StatusCode(
                StatusCodes.Status503ServiceUnavailable,
                "SubscriberService is currently unavailable.");

        }
    }
    
}