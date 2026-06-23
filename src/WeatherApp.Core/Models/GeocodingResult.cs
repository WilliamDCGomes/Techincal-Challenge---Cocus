namespace WeatherApp.Core.Models;

public sealed record GeocodingResult(
    string Name,
    double Latitude,
    double Longitude,
    string? Country,
    string? Admin1)
{
    public Coordinate Coordinate => new(Latitude, Longitude);

    public string DisplayName =>
        string.IsNullOrWhiteSpace(Country) ? Name : $"{Name}, {Country}";
}
