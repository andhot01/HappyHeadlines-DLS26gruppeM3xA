using Microsoft.EntityFrameworkCore;
using SubscriberService.Data;
using SubscriberService.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddSingleton<SubscriberQueuePublisher>();
builder.Services.AddSingleton<FeatureFlagState>();
builder.Services.AddHostedService<FeatureFlagRefresher>();

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

app.Use(async (HttpContext context, RequestDelegate next) =>
{
    var state = context.RequestServices.GetRequiredService<FeatureFlagState>();

    if (!state.Enabled)
    {
        context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
        await context.Response.WriteAsync("SubscriberService is currently disabled.");
        return;
    }

    await next(context);
});

app.MapControllers();
app.Run();