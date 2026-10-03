using CommentService.Data;
using CommentService.Repositories;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using Prometheus;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddDbContext<CommentDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("CommentDb")));

builder.Services.AddScoped<CommentRepository>();
builder.Services.AddScoped<ICommentRepository, CachedCommentRepository>();

builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
    ConnectionMultiplexer.Connect(
        builder.Configuration.GetConnectionString("CommentCache")
        ?? "localhost:6380,abortConnect=false"));

builder.Services
    .AddHttpClient("ProfanityService", client =>
    {
        client.BaseAddress = new Uri("http://profanity-service:8080");
    })
    .AddStandardResilienceHandler();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CommentDbContext>();
    db.Database.EnsureCreated();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapControllers();
app.MapMetrics();

app.Run();