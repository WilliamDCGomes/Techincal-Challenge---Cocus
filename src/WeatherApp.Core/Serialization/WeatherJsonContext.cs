using System.Text.Json.Serialization;
using WeatherApp.Core.Dtos;

namespace WeatherApp.Core.Serialization;

[JsonSourceGenerationOptions(
    PropertyNameCaseInsensitive = true,
    NumberHandling = JsonNumberHandling.AllowReadingFromString)]
[JsonSerializable(typeof(GeocodingResponseDto))]
[JsonSerializable(typeof(ForecastResponseDto))]
internal sealed partial class WeatherJsonContext : JsonSerializerContext
{
}
