using DraftService.Data;
using Microsoft.EntityFrameworkCore;
using Observability;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddHappyHeadlinesLogging();
builder.Services.AddHappyHeadlinesTracing("DraftService");

builder.Services.AddControllers();

builder.Services.AddDbContext<DraftDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DraftDatabase")));

var app = builder.Build();

// Create database tables if they don't exist
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<DraftDbContext>();
    db.Database.EnsureCreated();
}

app.MapControllers();

app.Run();