using System.Text.Json;
using ArticleService.Caching;
using ArticleService.Models;
using Prometheus;
using StackExchange.Redis;

namespace ArticleService.Repositories;

public class CachedArticleRepository : IArticleRepository
{
    private static readonly Counter CacheHits = Metrics.CreateCounter(
        "cache_hits_total",
        "Number of cache hits",
        new CounterConfiguration { LabelNames = new[] { "cache" } });

    private static readonly Counter CacheMisses = Metrics.CreateCounter(
        "cache_misses_total",
        "Number of cache misses",
        new CounterConfiguration { LabelNames = new[] { "cache" } });

    private readonly ArticleRepository _inner;
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<CachedArticleRepository> _logger;
    
    public CachedArticleRepository(
        ArticleRepository inner,
        IConnectionMultiplexer redis,
        ILogger<CachedArticleRepository> logger)
    {
        _inner = inner;
        _redis = redis;
        _logger = logger;
    }

    public IEnumerable<Article> GetAll(Region region)
    {
        try
        {
            var cached = _redis.GetDatabase().StringGet(ArticleCacheKeys.List(region));

            if (cached.HasValue)
            {
                CacheHits.WithLabels("article").Inc();
                return JsonSerializer.Deserialize<List<Article>>((string)cached!)!;
            }
        }
        catch (RedisException ex)
        {
            _logger.LogWarning(ex, "Article cache unavailable on read, falling back to database");
        }
        
        CacheMisses.WithLabels("article").Inc();
        return _inner.GetAll(region);
    }

    public Article? GetById(Guid id, Region region)
    {
        try
        {
            var cached = _redis.GetDatabase().StringGet(ArticleCacheKeys.Article(region, id));
            
            if (cached.HasValue)
            {
                CacheHits.WithLabels("article").Inc();
                return JsonSerializer.Deserialize<Article>((string)cached!);
            }
        }
        catch (RedisException ex)
        {
            _logger.LogWarning(ex, "Article cache unavailable on read, falling back to database");
        }
        
        CacheMisses.WithLabels("article").Inc();
        return _inner.GetById(id, region);
    }

    public Article Create(Article article)
    {
        var created = _inner.Create(article);

        try
        {
            var db = _redis.GetDatabase();

            if (DateTime.UtcNow - created.PublishedAt <= ArticleCacheKeys.MaxAge)
            {
                db.StringSet(
                    ArticleCacheKeys.Article(created.Region, created.Id),
                    JsonSerializer.Serialize(created),
                    ArticleCacheKeys.Ttl);
            }

            db.KeyDelete(ArticleCacheKeys.List(created.Region));
        }
        catch (RedisException ex)
        {
            _logger.LogWarning(ex, "Article cache unavailable on create");
        }

        return created;
    }

    public bool Update(Guid id, Region region, Article article)
    {
        var sucess = _inner.Update(id, region, article);

        if (sucess)
        {
            Invalidate(id, region);
        }
        
        return sucess;
    }

    public bool Delete(Guid id, Region region)
    {
        var sucess = _inner.Delete(id, region);

        if (sucess)
        {
            Invalidate(id, region);
        }
        
        return sucess;
    }

    private void Invalidate(Guid id, Region region)
    {
        try
        {
            var db = _redis.GetDatabase();
            db.KeyDelete(ArticleCacheKeys.Article(region, id));
            db.KeyDelete(ArticleCacheKeys.List(region));
        }
        catch (RedisException ex)
        {
            _logger.LogWarning(ex, "Article cache unavailable on invalidate");
        }
    }
}