using Observability;
using NewsletterService.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddHappyHeadlinesTracing("NewsletterService");

builder.Services.AddHostedService<NewsletterArticleConsumer>();

builder.Services.AddHttpClient<ArticleServiceClient>(client =>
{
    client.BaseAddress = new Uri(
        builder.Configuration["ArticleService:BaseUrl"]
        ?? "http://localhost:8080");
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();