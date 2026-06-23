using WeatherApp.Core.Services;

namespace WeatherApp.Tests.Services;

public class WeatherCodeDescriptionsTests
{
    [Theory]
    [InlineData(0, "Clear sky")]
    [InlineData(2, "Partly cloudy")]
    [InlineData(3, "Overcast")]
    [InlineData(45, "Fog")]
    [InlineData(61, "Slight rain")]
    [InlineData(71, "Slight snowfall")]
    [InlineData(95, "Thunderstorm")]
    [InlineData(99, "Thunderstorm with heavy hail")]
    public void Describe_KnownCode_ReturnsExpectedText(int code, string expected)
    {
        Assert.Equal(expected, WeatherCodeDescriptions.Describe(code));
    }

    [Theory]
    [InlineData(4)]
    [InlineData(44)]
    [InlineData(-1)]
    [InlineData(1000)]
    public void Describe_UnknownCode_FallsBackToUnknown(int code)
    {
        Assert.Equal(WeatherCodeDescriptions.Unknown, WeatherCodeDescriptions.Describe(code));
    }
}
