namespace ArticleService.Models;

public class Article
{
    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public Region Region { get; set; }

    public DateTime PublishedAt { get; set; }
}