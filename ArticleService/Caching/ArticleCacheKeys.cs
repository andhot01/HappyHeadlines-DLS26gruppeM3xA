using ArticleService.Models;

namespace ArticleService.Caching;

public class ArticleCacheKeys
{
    public static readonly TimeSpan MaxAge = TimeSpan.FromDays(14);
    public static readonly TimeSpan Ttl = TimeSpan.FromMinutes(30);
    public static string Article(Region region, Guid id) => $"article:{region}:{id}";
    public static string List(Region region) => $"articles:{region}";
}