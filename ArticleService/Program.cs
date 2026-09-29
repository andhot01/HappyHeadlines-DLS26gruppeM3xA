using ArticleService.Repositories;
using ArticleService.Data;
using Observability;
using ArticleService.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddOpenApi();
builder.Services.AddHappyHeadlinesTracing("ArticleService");

builder.Services.AddSingleton<IArticleRepository, ArticleRepository>();
builder.Services.AddSingleton<ArticleDbContextFactory>();
builder.Services.AddHostedService<ArticleQueueConsumer>();

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

app.Run();