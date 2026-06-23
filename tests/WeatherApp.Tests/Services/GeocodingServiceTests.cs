using System.Net;
using Microsoft.Extensions.Logging.Abstractions;
using WeatherApp.Core.Services;
using WeatherApp.Tests.Http;
using WeatherApp.Tests.TestData;

namespace WeatherApp.Tests.Services;

public class GeocodingServiceTests
{
    private static GeocodingService CreateService(StubHttpMessageHandler handler) =>
        new(handler.CreateClient(GeocodingService.BaseAddress), NullLogger<GeocodingService>.Instance);

    [Fact]
    public async Task SearchAsync_BuildsExpectedRequestUri()
    {
        var handler = StubHttpMessageHandler.Json(OpenMeteoSamples.GeocodingLisbon);
        var service = CreateService(handler);

        await service.SearchAsync("New York");

        var uri = handler.LastRequestUri!;
        Assert.Equal("geocoding-api.open-meteo.com", uri.Host);
        Assert.Equal("/v1/search", uri.AbsolutePath);
        Assert.Contains("name=New%20York", uri.Query);
        Assert.Contains("count=1", uri.Query);
        Assert.Contains("language=en", uri.Query);
        Assert.Contains("format=json", uri.Query);
    }

    [Fact]
    public async Task SearchAsync_ValidResponse_ReturnsMappedResult()
    {
        var service = CreateService(StubHttpMessageHandler.Json(OpenMeteoSamples.GeocodingLisbon));

        var result = await service.SearchAsync("Lisbon");

        Assert.NotNull(result);
        Assert.Equal("Lisbon", result!.Name);
        Assert.Equal("Portugal", result.Country);
        Assert.Equal(38.71667, result.Coordinate.Latitude, precision: 5);
    }

    [Fact]
    public async Task SearchAsync_NoResults_ReturnsNull()
    {
        var service = CreateService(StubHttpMessageHandler.Json(OpenMeteoSamples.GeocodingNoResults));

        var result = await service.SearchAsync("asdfghjkl");

        Assert.Null(result);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task SearchAsync_BlankCity_ReturnsNullWithoutHttpCall(string city)
    {
        var handler = StubHttpMessageHandler.Json(OpenMeteoSamples.GeocodingLisbon);
        var service = CreateService(handler);

        var result = await service.SearchAsync(city);

        Assert.Null(result);
        Assert.Equal(0, handler.RequestCount);
    }

    [Fact]
    public async Task SearchAsync_ServerError_ThrowsHttpRequestException()
    {
        var service = CreateService(StubHttpMessageHandler.Status(HttpStatusCode.InternalServerError));

        await Assert.ThrowsAsync<HttpRequestException>(() => service.SearchAsync("Lisbon"));
    }

    [Fact]
    public async Task SearchAsync_MalformedJson_ThrowsWeatherExceptionInvalidResponse()
    {
        var service = CreateService(StubHttpMessageHandler.Json("{ this is not valid json "));

        var ex = await Assert.ThrowsAsync<WeatherException>(() => service.SearchAsync("Lisbon"));
        Assert.Equal(Core.Enums.ErrorKind.InvalidResponse, ex.Kind);
    }

    [Fact]
    public async Task SearchAsync_CancelledToken_Throws()
    {
        var service = CreateService(StubHttpMessageHandler.Json(OpenMeteoSamples.GeocodingLisbon));
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => service.SearchAsync("Lisbon", cts.Token));
    }
}
