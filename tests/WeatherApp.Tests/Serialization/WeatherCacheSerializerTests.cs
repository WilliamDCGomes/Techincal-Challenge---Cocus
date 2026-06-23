using WeatherApp.Core.Models;
using WeatherApp.Core.Serialization;

namespace WeatherApp.Tests.Serialization;

public class WeatherCacheSerializerTests
{
    private static WeatherResult SampleWeather()
    {
        var current = new CurrentWeather(24.3, 25.1, 2, "Partly cloudy", 55, 12.4, IsDay: true);
        var days = new List<DailyForecast>
        {
            new(new DateOnly(2026, 6, 23), 0, "Clear sky", 28, 17),
            new(new DateOnly(2026, 6, 24), 61, "Slight rain", 24, 16),
        };
        return new WeatherResult(current, days);
    }

    [Fact]
    public void Weather_RoundTrips()
    {
        var stamp = new DateTimeOffset(2026, 6, 23, 12, 0, 0, TimeSpan.Zero);
        var cached = new CachedWeather(SampleWeather(), stamp);

        var json = WeatherCacheSerializer.SerializeWeather(cached);
        var restored = WeatherCacheSerializer.DeserializeWeather(json);

        Assert.NotNull(restored);
        Assert.Equal(stamp, restored!.CachedAtUtc);
        Assert.Equal(24.3, restored.Weather.Current.Temperature, precision: 3);
        Assert.Equal("Partly cloudy", restored.Weather.Current.Description);
        Assert.Equal(2, restored.Weather.Daily.Count);
        Assert.Equal(new DateOnly(2026, 6, 24), restored.Weather.Daily[1].Date);
        Assert.Equal(61, restored.Weather.Daily[1].WeatherCode);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("{ not valid json")]
    public void DeserializeWeather_MissingOrCorrupt_ReturnsNull(string? json)
    {
        Assert.Null(WeatherCacheSerializer.DeserializeWeather(json));
    }

    [Fact]
    public void Recents_RoundTrip()
    {
        IReadOnlyList<GeocodingResult> recents = new[]
        {
            new GeocodingResult("Lisbon", 38.71667, -9.13333, "Portugal", "Lisbon"),
            new GeocodingResult("Porto", 41.14961, -8.61099, "Portugal", "Porto"),
        };

        var json = WeatherCacheSerializer.SerializeRecents(recents);
        var restored = WeatherCacheSerializer.DeserializeRecents(json);

        Assert.Equal(2, restored.Count);
        Assert.Equal("Lisbon", restored[0].Name);
        Assert.Equal(38.71667, restored[0].Latitude, precision: 5);
        Assert.Equal("Porto", restored[1].Name);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("not json")]
    public void DeserializeRecents_MissingOrCorrupt_ReturnsEmpty(string? json)
    {
        Assert.Empty(WeatherCacheSerializer.DeserializeRecents(json));
    }

    [Fact]
    public void IsExpired_RespectsTtl()
    {
        var now = new DateTimeOffset(2026, 6, 23, 12, 0, 0, TimeSpan.Zero);
        var fresh = new CachedWeather(SampleWeather(), now.AddMinutes(-10));
        var stale = new CachedWeather(SampleWeather(), now.AddMinutes(-20));

        Assert.False(fresh.IsExpired(now));
        Assert.True(stale.IsExpired(now));
    }
}
