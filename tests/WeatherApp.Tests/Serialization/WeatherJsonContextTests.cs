using System.Text.Json;
using WeatherApp.Core.Dtos;
using WeatherApp.Core.Serialization;
using WeatherApp.Tests.TestData;

namespace WeatherApp.Tests.Serialization;

public class WeatherJsonContextTests
{
    [Fact]
    public void Geocoding_WithMatch_DeserializesResult()
    {
        var dto = JsonSerializer.Deserialize(
            OpenMeteoSamples.GeocodingLisbon,
            WeatherJsonContext.Default.GeocodingResponseDto);

        Assert.NotNull(dto);
        Assert.NotNull(dto!.Results);
        var result = Assert.Single(dto.Results!);
        Assert.Equal("Lisbon", result.Name);
        Assert.Equal(38.71667, result.Latitude, precision: 5);
        Assert.Equal(-9.13333, result.Longitude, precision: 5);
        Assert.Equal("Portugal", result.Country);
        Assert.Equal("Lisbon", result.Admin1);
    }

    [Fact]
    public void Geocoding_WithoutResultsKey_LeavesResultsNull()
    {
        var dto = JsonSerializer.Deserialize(
            OpenMeteoSamples.GeocodingNoResults,
            WeatherJsonContext.Default.GeocodingResponseDto);

        Assert.NotNull(dto);
        Assert.Null(dto!.Results);
    }

    [Fact]
    public void Forecast_Success_DeserializesCurrentAndDailyArrays()
    {
        var dto = JsonSerializer.Deserialize(
            OpenMeteoSamples.ForecastLisbon,
            WeatherJsonContext.Default.ForecastResponseDto);

        Assert.NotNull(dto);
        Assert.True(dto!.Error is null or false);

        Assert.NotNull(dto.Current);
        Assert.Equal(24.3, dto.Current!.Temperature, precision: 3);
        Assert.Equal(2, dto.Current.WeatherCode);
        Assert.Equal(55, dto.Current.RelativeHumidity);
        Assert.Equal(1, dto.Current.IsDay);

        Assert.NotNull(dto.Daily);
        Assert.Equal(7, dto.Daily!.Time!.Length);
        Assert.Equal(7, dto.Daily.WeatherCode!.Length);
        Assert.Equal(7, dto.Daily.TemperatureMax!.Length);
        Assert.Equal(7, dto.Daily.TemperatureMin!.Length);
    }

    [Fact]
    public void Forecast_ErrorBody_DeserializesErrorFlag()
    {
        var dto = JsonSerializer.Deserialize(
            OpenMeteoSamples.ForecastError,
            WeatherJsonContext.Default.ForecastResponseDto);

        Assert.NotNull(dto);
        Assert.True(dto!.Error);
        Assert.Contains("Latitude", dto.Reason);
    }
}
