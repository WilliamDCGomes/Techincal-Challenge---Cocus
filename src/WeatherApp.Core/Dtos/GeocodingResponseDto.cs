using System.Text.Json.Serialization;

namespace WeatherApp.Core.Dtos;

internal sealed class GeocodingResponseDto
{
    [JsonPropertyName("results")]
    public List<GeocodingResultDto>? Results { get; init; }

    [JsonPropertyName("generationtime_ms")]
    public double? GenerationTimeMs { get; init; }
}

internal sealed class GeocodingResultDto
{
    [JsonPropertyName("name")]
    public string? Name { get; init; }

    [JsonPropertyName("latitude")]
    public double Latitude { get; init; }

    [JsonPropertyName("longitude")]
    public double Longitude { get; init; }

    [JsonPropertyName("country")]
    public string? Country { get; init; }

    [JsonPropertyName("admin1")]
    public string? Admin1 { get; init; }

    [JsonPropertyName("country_code")]
    public string? CountryCode { get; init; }

    [JsonPropertyName("timezone")]
    public string? Timezone { get; init; }
}
