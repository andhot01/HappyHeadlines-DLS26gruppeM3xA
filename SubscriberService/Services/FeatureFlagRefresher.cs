using System.Text.Json;

namespace SubscriberService.Services;

public class FeatureFlagState
{
    public volatile bool Enabled;
}

public class FeatureFlagRefresher : BackgroundService
{
    private readonly FeatureFlagState _state;
    private readonly IConfiguration _config;
    private readonly HttpClient _http = new();

    public FeatureFlagRefresher(FeatureFlagState state, IConfiguration config)
    {
        _state = state;
        _config = config;
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        var url = $"{_config["FeatureHub:Url"]}/features?sdkUrl={_config["FeatureHub:ApiKey"]}";

        while (!ct.IsCancellationRequested)
        {
            try
            {
                var json = await _http.GetStringAsync(url, ct);
                using var doc = JsonDocument.Parse(json);

                foreach (var env in doc.RootElement.EnumerateArray())
                foreach (var f in env.GetProperty("features").EnumerateArray())
                {
                    if (f.GetProperty("key").GetString() != "SubscriberServiceEnabled") continue;

                    var value = f.TryGetProperty("value", out var v) && v.ValueKind == JsonValueKind.True;
                    if (value != _state.Enabled)
                        Console.WriteLine($"SubscriberServiceEnabled changed to {value}");
                    _state.Enabled = value;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"FeatureHub refresh failed: {ex.Message}");
            }

            await Task.Delay(TimeSpan.FromSeconds(5), ct);
        }
    }
}