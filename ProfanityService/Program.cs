using Microsoft.EntityFrameworkCore;
using ProfanityService.Data;
using ProfanityService.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddDbContext<ProfanityDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("ProfanityDb")));

builder.Services.AddScoped<IProfanityRepository, ProfanityRepository>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ProfanityDbContext>();
    db.Database.EnsureCreated();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapControllers();

app.Run();