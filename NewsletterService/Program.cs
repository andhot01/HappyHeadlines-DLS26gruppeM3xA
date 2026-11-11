using FeatureHubSDK;
using Observability;
using NewsletterService.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddHappyHeadlinesTracing("NewsletterService");

builder.Services.AddHostedService<NewsletterArticleConsumer>();
builder.Services.AddHostedService<SubscriberConsumer>();

builder.Services.AddSingleton<IFeatureHubConfig>(_ =>
    new EdgeFeatureHubConfig(
        builder.Configuration["FeatureHub:Url"] ?? "http://localhost:8090",
        builder.Configuration["FeatureHub:ApiKey"] ?? ""));
builder.Services.AddSingleton<IFeatureToggle, FeatureHubToggle>();

builder.Services.AddHostedService<NewsletterArticleConsumer>();
builder.Services.AddHostedService<SubscriberConsumer>();

builder.Services.AddHttpClient<ArticleServiceClient>(client =>
{
    client.BaseAddress = new Uri(
        builder.Configuration["ArticleService:BaseUrl"]
        ?? "http://localhost:8080");
});

builder.Services.AddHttpClient<SubscriberServiceClient>(client =>
{
    client.BaseAddress = new Uri(
        builder.Configuration["SubscriberService:BaseUrl"]
        ?? "http://localhost:8087");
})
.AddStandardResilienceHandler();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();