using Microsoft.AspNetCore.Mvc;
using NewsletterService.Services;

namespace NewsletterService.Controllers;

[ApiController]
[Route("api/newsletter")]
public class NewsletterController : ControllerBase
{
    private readonly ArticleServiceClient _articleServiceClient;

    public NewsletterController(ArticleServiceClient articleServiceClient)
    {
        _articleServiceClient = articleServiceClient;
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
}