using FeatureHubSDK;
using Microsoft.EntityFrameworkCore;
using SubscriberService.Data;
using SubscriberService.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddSingleton<SubscriberQueuePublisher>();

builder.Services.AddSingleton<IFeatureHubConfig>(_ =>
    new EdgeFeatureHubConfig(
        builder.Configuration["FeatureHub:Url"] ?? "http://localhost:8090",
        builder.Configuration["FeatureHub:ApiKey"] ?? ""));

builder.Services.AddDbContext<SubscriberDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("SubscriberDb")));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<SubscriberDbContext>();
    db.Database.EnsureCreated();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Feature toggle: hele SubscriberService er skjult, når flaget er slukket
app.Use(async (HttpContext context, RequestDelegate next) =>
{
    var config = context.RequestServices.GetRequiredService<IFeatureHubConfig>();

    bool enabled;
    try
    {
        var fh = await config.NewContext().Build();
        enabled = fh["SubscriberServiceEnabled"].IsEnabled;
    }
    catch
    {
        enabled = false; // fail closed
    }

    if (!enabled)
    {
        context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
        await context.Response.WriteAsync("SubscriberService is currently disabled.");
        return;
    }

    await next(context);
});

app.MapControllers();
app.Run();