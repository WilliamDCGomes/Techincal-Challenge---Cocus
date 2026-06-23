namespace WeatherApp.Core.Models;

public sealed record CachedWeather(WeatherResult Weather, DateTimeOffset CachedAtUtc)
{
    public static readonly TimeSpan DefaultTtl = TimeSpan.FromMinutes(15);

    public bool IsExpired(DateTimeOffset now) => IsExpired(DefaultTtl, now);

    public bool IsExpired(TimeSpan ttl, DateTimeOffset now) => now - CachedAtUtc > ttl;
}
