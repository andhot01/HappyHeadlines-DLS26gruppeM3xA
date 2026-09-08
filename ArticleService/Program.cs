using ArticleService.Repositories;
using ArticleService.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddOpenApi();

builder.Services.AddSingleton<IArticleRepository, ArticleRepository>();
builder.Services.AddSingleton<ArticleDbContextFactory>();

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