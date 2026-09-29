namespace NewsletterService.Models;

public class ArticleResponse
{
    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public int Region { get; set; }

    public DateTime PublishedAt { get; set; }
}