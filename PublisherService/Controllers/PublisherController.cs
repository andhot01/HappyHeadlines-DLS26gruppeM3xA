using Microsoft.AspNetCore.Mvc;
using PublisherService.Models;
using PublisherService.Services;

namespace PublisherService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PublisherController : ControllerBase
{
    private readonly ArticleQueuePublisher _queuePublisher;

    public PublisherController(ArticleQueuePublisher queuePublisher)
    {
        _queuePublisher = queuePublisher;
    }

    [HttpPost("publish")]
    public async Task<IActionResult> Publish(PublishedArticle article)
    {
        article.Id = Guid.NewGuid();
        article.PublishedAt = DateTime.UtcNow;

        await _queuePublisher.PublishAsync(article);

        return Accepted(article);
    }
}