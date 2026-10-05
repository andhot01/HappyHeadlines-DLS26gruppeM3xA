using System.Text.Json;
using ArticleService.Caching;
using ArticleService.Models;
using ArticleService.Repositories;
using StackExchange.Redis;

namespace ArticleService.Services;

public class ArticleCacheWarmer : BackgroundService
{
    private const string LockKey = "articles:warmer:lock";

    private readonly ArticleRepository _repository;
    private readonly IConnectionMultiplexer _redis;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ArticleCacheWarmer> _logger;

    public ArticleCacheWarmer(
        ArticleRepository repository,
        IConnectionMultiplexer redis,
        IConfiguration configuration,
        ILogger<ArticleCacheWarmer> logger)
    {
        _repository = repository;
        _redis = redis;
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var interval = TimeSpan.FromSeconds(
            _configuration.GetValue("ArticleCache:RefreshIntervalSeconds", 600));

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                Warm(interval);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Article cache warmin failed");
            }

            try
            {
                await Task.Delay(interval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    private void Warm(TimeSpan interval)
    {
        var db = _redis.GetDatabase();
        
        var gotLock = db.StringSet(
            LockKey,
            Environment.MachineName,
            interval - TimeSpan.FromSeconds(1),
            When.NotExists);

        if (!gotLock)
        {
            return;
        }

        var cutoff = DateTime.UtcNow - ArticleCacheKeys.MaxAge;

        foreach (var region in Enum.GetValues<Region>().Where(ArticleCacheKeys.IsCached))
        {
            var recent = _repository
                .GetAll(region)
                .Where(a => a.PublishedAt >= cutoff)
                .ToList();

            foreach (var article in recent)
            {
                db.StringSet(
                    ArticleCacheKeys.Article(region, article.Id),
                    JsonSerializer.Serialize(article),
                    ArticleCacheKeys.Ttl);
            }
            
            db.StringSet(
                ArticleCacheKeys.List(region),
                JsonSerializer.Serialize(recent),
                ArticleCacheKeys.Ttl);
            
            _logger.LogInformation(
                "Warmed article cache for {region}: {count} articles",
                region,
                recent.Count);
        }
    }
}