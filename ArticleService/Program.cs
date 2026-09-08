using ArticleService.Repositories;
using ArticleService.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddOpenApi();

builder.Services.AddSingleton<IArticleRepository, ArticleRepository>();
builder.Services.AddSingleton<ArticleDbContextFactory>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();