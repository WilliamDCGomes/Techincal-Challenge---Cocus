using WeatherApp.Core.Dtos;
using WeatherApp.Core.Mapping;

namespace WeatherApp.Tests.Mapping;

public class WeatherMapperGuardTests
{
    private static CurrentWeatherDto CurrentWith(double temperature) => new()
    {
        Time = "2026-06-23T12:00",
        Temperature = temperature,
        WeatherCode = 0,
        ApparentTemperature = 20.0,
        RelativeHumidity = 50,
        WindSpeed = 5.0,
        IsDay = 1,
    };

    private static DailyForecastDto ValidDaily() => new()
    {
        Time = ["2026-06-23"],
        WeatherCode = [0],
        TemperatureMax = [20.0],
        TemperatureMin = [10.0],
    };

    [Fact]
    public void ToWeatherResult_MalformedDailyDate_ThrowsInvalidOperation()
    {
        var dto = new ForecastResponseDto
        {
            Current = CurrentWith(20.0),
            Daily = new DailyForecastDto
            {
                Time = ["2026-13-99"],
                WeatherCode = [0],
                TemperatureMax = [20.0],
                TemperatureMin = [10.0],
            },
        };

        Assert.Throws<InvalidOperationException>(() => WeatherMapper.ToWeatherResult(dto));
    }

    [Fact]
    public void ToWeatherResult_NullDailyDateElement_ThrowsInvalidOperation()
    {
        var dto = new ForecastResponseDto
        {
            Current = CurrentWith(20.0),
            Daily = new DailyForecastDto
            {
                Time = [null!],
                WeatherCode = [0],
                TemperatureMax = [20.0],
                TemperatureMin = [10.0],
            },
        };

        Assert.Throws<InvalidOperationException>(() => WeatherMapper.ToWeatherResult(dto));
    }

    [Fact]
    public void ToWeatherResult_NaNCurrentTemperature_ThrowsInvalidOperation()
    {
        var dto = new ForecastResponseDto { Current = CurrentWith(double.NaN), Daily = ValidDaily() };

        Assert.Throws<InvalidOperationException>(() => WeatherMapper.ToWeatherResult(dto));
    }

    [Fact]
    public void ToWeatherResult_InfiniteDailyMax_ThrowsInvalidOperation()
    {
        var dto = new ForecastResponseDto
        {
            Current = CurrentWith(20.0),
            Daily = new DailyForecastDto
            {
                Time = ["2026-06-23"],
                WeatherCode = [0],
                TemperatureMax = [double.PositiveInfinity],
                TemperatureMin = [10.0],
            },
        };

        Assert.Throws<InvalidOperationException>(() => WeatherMapper.ToWeatherResult(dto));
    }
}
