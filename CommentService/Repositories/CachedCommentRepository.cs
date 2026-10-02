using System.Text.Json;
using CommentService.Models;
using Prometheus;
using StackExchange.Redis;

namespace CommentService.Repositories;

public class CachedCommentRepository : ICommentRepository
{
    private const int MaxCachedArticles = 30;
    private const string Lrukey = "comments:lru";

    private static readonly Counter CacheHits = Metrics.CreateCounter(
        "cache_hits_total",
        "Number of cache hits",
        new CounterConfiguration { LabelNames = new[] { "cache" } });
    
    private static readonly Counter CacheMisses = Metrics.CreateCounter(
        "cache_misses_total",
        "Number of cache misses",
        new CounterConfiguration { LabelNames = new[] { "cache" } });
    
    private const string StoreScript = @"
        redis.call('SET', KEYS[1], ARGV[1])
        redis.call('ZADD', KEYS[2], ARGV[2], ARGV[3])
        local excess = redis.call('ZCARD', KEYS[2]) - tonumber(ARGV[4])
        if excess > 0 then
            local victims = redis.call('ZRANGE', KEYS[2], 0, excess - 1)
            for _, id in ipairs(victims) do
                redis.call('DEL', 'comments:' .. id)
                redis.call('ZREM', KEYS[2], id)
            end
        end
        return excess";

    private readonly CommentRepository _inner;
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<CachedCommentRepository> _logger;
    
    public CachedCommentRepository(
        CommentRepository inner,
        IConnectionMultiplexer redis,
        ILogger<CachedCommentRepository> logger)
    {
        _inner = inner;
        _redis = redis;
        _logger = logger;
    }
    
    private static string CommentsKey(Guid articleId) => $"comments:{articleId}";
    private static long NowScore() => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

    public IEnumerable<Comment> GetByArticleId(Guid articleId)
    {
        try
        {
            var db = _redis.GetDatabase();
            var cached = db.StringGet(CommentsKey(articleId));

            if (cached.HasValue)
            {
                CacheHits.WithLabels("comment").Inc();
                
                db.SortedSetAdd(Lrukey, articleId.ToString(), NowScore());
                return JsonSerializer.Deserialize<List<Comment>>((string)cached!)!;
            }
        }
        catch (RedisException ex)
        {
            _logger.LogWarning(ex, "Comment cache unavailable on read, falling back to database");
        }

        CacheMisses.WithLabels("comment").Inc();
        var comments = _inner.GetByArticleId(articleId).ToList();

        try
        {
            _redis.GetDatabase().ScriptEvaluate(
                StoreScript,
                new RedisKey[] { CommentsKey(articleId), Lrukey },
                new RedisValue[]
                {
                    JsonSerializer.Serialize(comments),
                    NowScore(),
                    articleId.ToString(),
                    MaxCachedArticles
                });
        }
        catch (RedisException ex)
        {
            _logger.LogWarning(ex, "Comment cache unavailable on write");
        }
        
        return comments;
    }
    
    public Comment? GetById(Guid id) => _inner.GetById(id);

    public Comment Create(Comment comment)
    {
        var created = _inner.Create(comment);
        Invalidate(created.ArticleId);
        return created;
    }

    public bool Delete(Guid id)
    {
        var existing = _inner.GetById(id);
        var sucess = _inner.Delete(id);

        if (sucess && existing != null)
        {
            Invalidate(existing.ArticleId);
        }
        return sucess;
    }

    private void Invalidate(Guid articleId)
    {
        try
        {
            var db = _redis.GetDatabase();
            db.KeyDelete(CommentsKey(articleId));
            db.SortedSetRemove(Lrukey, articleId.ToString());
        }
        catch (RedisException ex)
        {
            _logger.LogWarning(ex, "Comment cache unavailable on invalidate");
        }
    }
}