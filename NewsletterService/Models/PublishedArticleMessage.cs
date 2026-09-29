namespace NewsletterService.Models;

public class PublishedArticleMessage
{
    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public string Region { get; set; } = string.Empty;

    public DateTime PublishedAt { get; set; }
}