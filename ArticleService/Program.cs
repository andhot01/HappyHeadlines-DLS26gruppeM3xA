using ArticleService.Repositories;
using ArticleService.Data;
using Observability;
using ArticleService.Services;
using Prometheus;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddOpenApi();
builder.Services.AddHappyHeadlinesTracing("ArticleService");

builder.Services.AddSingleton<ArticleRepository>();
builder.Services.AddSingleton<IArticleRepository, CachedArticleRepository>();
builder.Services.AddSingleton<ArticleDbContextFactory>();
builder.Services.AddHostedService<ArticleQueueConsumer>();

builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
    ConnectionMultiplexer.Connect(
        builder.Configuration.GetConnectionString("ArticleCache")
        ?? "localhost:6379,abortConnect=false"));

var app = builder.Build();


//makes sure dbs are created on startup
using (var scope = app.Services.CreateScope())
{
    var factory =
        scope.ServiceProvider.GetRequiredService<ArticleDbContextFactory>();

    factory.EnsureDatabasesCreated();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapControllers();
app.MapMetrics();

app.Run();