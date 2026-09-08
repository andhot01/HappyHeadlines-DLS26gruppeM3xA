using ArticleService.Models;
using Microsoft.EntityFrameworkCore;

namespace ArticleService.Data;

public class ArticleDbContextFactory
{
    private readonly IConfiguration _configuration;

    public ArticleDbContextFactory(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public ArticleDbContext Create(Region region)
    {
        var connectionStringName = region switch
        {
            Region.Africa => "AfricaDb",
            Region.Antarctica => "AntarcticaDb",
            Region.Asia => "AsiaDb",
            Region.Europe => "EuropeDb",
            Region.NorthAmerica => "NorthAmericaDb",
            Region.SouthAmerica => "SouthAmericaDb",
            Region.Oceania => "OceaniaDb",
            Region.Global => "GlobalDb",
            _ => throw new ArgumentOutOfRangeException(
                nameof(region),
                region,
                "Unknown region")
        };

        var connectionString =
            _configuration.GetConnectionString(connectionStringName);

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                $"Connection string '{connectionStringName}' was not found.");
        }

        var options = new DbContextOptionsBuilder<ArticleDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new ArticleDbContext(options);
    }

    public void EnsureDatabasesCreated()
    {
        foreach (Region region in Enum.GetValues<Region>())
        {
            using var context = Create(region);
            context.Database.EnsureCreated();
        }
    
    }
}