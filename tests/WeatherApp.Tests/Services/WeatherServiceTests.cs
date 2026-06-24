using System.Net;
using Microsoft.Extensions.Logging.Abstractions;
using WeatherApp.Core.Enums;
using WeatherApp.Core.Models;
using WeatherApp.Core.Services;
using WeatherApp.Tests.Http;
using WeatherApp.Tests.TestData;

namespace WeatherApp.Tests.Services;

public class WeatherServiceTests
{
    private static readonly Coordinate Lisbon = new(38.71667, -9.13333);

    private static WeatherService CreateService(StubHttpMessageHandler handler) =>
        new(handler.CreateClient(WeatherService.BaseAddress), NullLogger<WeatherService>.Instance);

    [Fact]
    public async Task GetForecastAsync_BuildsExpectedRequestUri()
    {
        var handler = StubHttpMessageHandler.Json(OpenMeteoSamples.ForecastLisbon);
        var service = CreateService(handler);

        await service.GetForecastAsync(Lisbon);

        var uri = handler.LastRequestUri!;
        Assert.Equal("api.open-meteo.com", uri.Host);
        Assert.Equal("/v1/forecast", uri.AbsolutePath);
        Assert.Contains("latitude=38.71667", uri.Query);
        Assert.Contains("longitude=-9.13333", uri.Query);
        Assert.Contains("timezone=auto", uri.Query);
        Assert.Contains("forecast_days=7", uri.Query);
        Assert.Contains("current=temperature_2m", uri.Query);
        Assert.Contains("daily=weather_code", uri.Query);
    }

    [Fact]
    public async Task GetForecastAsync_ValidResponse_MapsCurrentAndSevenDays()
    {
        var service = CreateService(StubHttpMessageHandler.Json(OpenMeteoSamples.ForecastLisbon));

        var result = await service.GetForecastAsync(Lisbon);

        Assert.Equal(24.3, result.Current.Temperature, precision: 3);
        Assert.Equal("Partly cloudy", result.Current.Description);
        Assert.Equal(7, result.Daily.Count);
        Assert.Equal(new DateOnly(2026, 6, 23), result.Daily[0].Date);
    }

    [Fact]
    public async Task GetForecastAsync_ApiErrorBody_ThrowsInvalidResponseWithReason()
    {
        var service = CreateService(
            StubHttpMessageHandler.Json(OpenMeteoSamples.ForecastError, HttpStatusCode.BadRequest));

        var ex = await Assert.ThrowsAsync<WeatherException>(() => service.GetForecastAsync(Lisbon));
        Assert.Equal(ErrorKind.InvalidResponse, ex.Kind);
        Assert.Contains("Latitude", ex.Message);
    }

    [Fact]
    public async Task GetForecastAsync_MalformedJson_ThrowsInvalidResponse()
    {
        var service = CreateService(StubHttpMessageHandler.Json("<html>not json</html>"));

        var ex = await Assert.ThrowsAsync<WeatherException>(() => service.GetForecastAsync(Lisbon));
        Assert.Equal(ErrorKind.InvalidResponse, ex.Kind);
    }

    [Fact]
    public async Task GetForecastAsync_ServerErrorWithoutErrorBody_ThrowsHttpRequestException()
    {
        var service = CreateService(
            StubHttpMessageHandler.Status(HttpStatusCode.InternalServerError, "upstream down"));

        await Assert.ThrowsAsync<HttpRequestException>(() => service.GetForecastAsync(Lisbon));
    }

    [Fact]
    public async Task GetForecastAsync_CancelledToken_Throws()
    {
        var service = CreateService(StubHttpMessageHandler.Json(OpenMeteoSamples.ForecastLisbon));
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => service.GetForecastAsync(Lisbon, cts.Token));
    }

    [Fact]
    public async Task GetForecastAsync_MalformedDailyDate_ThrowsInvalidResponse()
    {
        const string json =
            """
            {
              "current": { "time": "2026-06-23T12:00", "temperature_2m": 20.0, "weather_code": 0,
                "apparent_temperature": 20.0, "relative_humidity_2m": 50, "wind_speed_10m": 5.0, "is_day": 1 },
              "daily": { "time": ["not-a-date"], "weather_code": [0],
                "temperature_2m_max": [20.0], "temperature_2m_min": [10.0] }
            }
            """;
        var service = CreateService(StubHttpMessageHandler.Json(json));

        var ex = await Assert.ThrowsAsync<WeatherException>(() => service.GetForecastAsync(Lisbon));
        Assert.Equal(ErrorKind.InvalidResponse, ex.Kind);
    }
}
