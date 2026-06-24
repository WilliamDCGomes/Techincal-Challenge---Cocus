using System.Globalization;
using WeatherApp.Core.Models;

namespace WeatherApp.Tests.Models;

public class DailyForecastTests
{
    [Theory]
    [InlineData("pt-BR")]
    [InlineData("fr-FR")]
    [InlineData("ja-JP")]
    [InlineData("en-US")]
    public void DayLabel_IsEnglishAbbreviation_RegardlessOfCurrentCulture(string culture)
    {
        var original = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo(culture);
            var thursday = new DailyForecast(new DateOnly(2026, 6, 25), 0, "Clear sky", 20, 10);

            Assert.Equal("Thu", thursday.DayLabel);
        }
        finally
        {
            CultureInfo.CurrentCulture = original;
        }
    }
}
