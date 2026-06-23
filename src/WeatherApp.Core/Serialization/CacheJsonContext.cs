using System.Text.Json.Serialization;
using WeatherApp.Core.Models;

namespace WeatherApp.Core.Serialization;

[JsonSourceGenerationOptions(WriteIndented = false)]
[JsonSerializable(typeof(CachedWeather))]
[JsonSerializable(typeof(List<GeocodingResult>))]
internal sealed partial class CacheJsonContext : JsonSerializerContext
{
}
