using WeatherApp.Core.Services;

namespace WeatherApp.Tests.Services;

public class WeatherIconsTests
{
    [Fact]
    public void Glyph_ClearSky_IsSunByDay_AndMoonByNight()
    {
        Assert.Equal("☀️", WeatherIcons.Glyph(0, isDay: true));
        Assert.Equal("🌙", WeatherIcons.Glyph(0, isDay: false));
    }

    [Theory]
    [InlineData(3)]
    [InlineData(61)]
    [InlineData(71)]
    [InlineData(95)]
    public void Glyph_CloudsRainSnowStorms_LookTheSameDayAndNight(int weatherCode)
    {
        Assert.Equal(
            WeatherIcons.Glyph(weatherCode, isDay: true),
            WeatherIcons.Glyph(weatherCode, isDay: false));
    }
}
