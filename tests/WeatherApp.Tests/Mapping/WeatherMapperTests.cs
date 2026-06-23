using System.Text.Json;
using WeatherApp.Core.Dtos;
using WeatherApp.Core.Mapping;
using WeatherApp.Core.Serialization;
using WeatherApp.Tests.TestData;

namespace WeatherApp.Tests.Mapping;

public class WeatherMapperTests
{
    private static GeocodingResponseDto DeserializeGeocoding(string json) =>
        JsonSerializer.Deserialize(json, WeatherJsonContext.Default.GeocodingResponseDto)!;

    private static ForecastResponseDto DeserializeForecast(string json) =>
        JsonSerializer.Deserialize(json, WeatherJsonContext.Default.ForecastResponseDto)!;

    [Fact]
    public void ToGeocodingResult_WithMatch_MapsNameCountryAndCoordinate()
    {
        var dto = DeserializeGeocoding(OpenMeteoSamples.GeocodingLisbon);

        var result = WeatherMapper.ToGeocodingResult(dto);

        Assert.NotNull(result);
        Assert.Equal("Lisbon", result!.Name);
        Assert.Equal("Portugal", result.Country);
        Assert.Equal(38.71667, result.Coordinate.Latitude, precision: 5);
        Assert.Equal(-9.13333, result.Coordinate.Longitude, precision: 5);
        Assert.Equal("Lisbon, Portugal", result.DisplayName);
    }

    [Fact]
    public void ToGeocodingResult_NoResults_ReturnsNull()
    {
        var dto = DeserializeGeocoding(OpenMeteoSamples.GeocodingNoResults);

        var result = WeatherMapper.ToGeocodingResult(dto);

        Assert.Null(result);
    }

    [Fact]
    public void ToWeatherResult_MapsCurrentConditions()
    {
        var dto = DeserializeForecast(OpenMeteoSamples.ForecastLisbon);

        var result = WeatherMapper.ToWeatherResult(dto);

        Assert.Equal(24.3, result.Current.Temperature, precision: 3);
        Assert.Equal(25.1, result.Current.ApparentTemperature, precision: 3);
        Assert.Equal(2, result.Current.WeatherCode);
        Assert.Equal("Partly cloudy", result.Current.Description);
        Assert.Equal(55, result.Current.RelativeHumidity);
        Assert.Equal(12.4, result.Current.WindSpeed, precision: 3);
        Assert.True(result.Current.IsDay);
    }

    [Fact]
    public void ToWeatherResult_ZipsParallelDailyArrays_PreservingOrderAndCount()
    {
        var dto = DeserializeForecast(OpenMeteoSamples.ForecastLisbon);

        var result = WeatherMapper.ToWeatherResult(dto);

        Assert.Equal(7, result.Daily.Count);

        var first = result.Daily[0];
        Assert.Equal(new DateOnly(2026, 6, 23), first.Date);
        Assert.Equal(2, first.WeatherCode);
        Assert.Equal("Partly cloudy", first.Description);
        Assert.Equal(28.1, first.TemperatureMax, precision: 3);
        Assert.Equal(17.2, first.TemperatureMin, precision: 3);

        Assert.Equal(new DateOnly(2026, 6, 25), result.Daily[2].Date);
        Assert.Equal(61, result.Daily[2].WeatherCode);
    }

    [Fact]
    public void ToWeatherResult_UnknownDailyCode_FallsBackToUnknownDescription()
    {
        var dto = DeserializeForecast(OpenMeteoSamples.ForecastLisbon);

        var result = WeatherMapper.ToWeatherResult(dto);

        var lastDay = result.Daily[^1];
        Assert.Equal(4, lastDay.WeatherCode);
        Assert.Equal("Unknown", lastDay.Description);
    }

    [Fact]
    public void ToWeatherResult_MissingCurrent_Throws()
    {
        var dto = DeserializeForecast(
            """
            { "daily": { "time": ["2026-06-23"], "weather_code": [0], "temperature_2m_max": [20.0], "temperature_2m_min": [10.0] } }
            """);

        Assert.Throws<InvalidOperationException>(() => WeatherMapper.ToWeatherResult(dto));
    }
}
