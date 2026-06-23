namespace WeatherApp.Tests.TestData;

internal static class OpenMeteoSamples
{
    public const string GeocodingLisbon =
        """
        {
          "results": [
            {
              "id": 2267057,
              "name": "Lisbon",
              "latitude": 38.71667,
              "longitude": -9.13333,
              "elevation": 16.0,
              "feature_code": "PPLC",
              "country_code": "PT",
              "timezone": "Europe/Lisbon",
              "country": "Portugal",
              "admin1": "Lisbon"
            }
          ],
          "generationtime_ms": 0.61
        }
        """;

    public const string GeocodingNoResults =
        """
        { "generationtime_ms": 0.22 }
        """;

    public const string ForecastLisbon =
        """
        {
          "latitude": 38.71667,
          "longitude": -9.13333,
          "generationtime_ms": 0.08,
          "utc_offset_seconds": 0,
          "timezone": "Europe/Lisbon",
          "timezone_abbreviation": "WET",
          "elevation": 16.0,
          "current_units": {
            "time": "iso8601",
            "interval": "seconds",
            "temperature_2m": "°C",
            "weather_code": "wmo code",
            "apparent_temperature": "°C",
            "relative_humidity_2m": "%",
            "wind_speed_10m": "km/h",
            "is_day": ""
          },
          "current": {
            "time": "2026-06-23T12:00",
            "interval": 900,
            "temperature_2m": 24.3,
            "weather_code": 2,
            "apparent_temperature": 25.1,
            "relative_humidity_2m": 55,
            "wind_speed_10m": 12.4,
            "is_day": 1
          },
          "daily_units": {
            "time": "iso8601",
            "weather_code": "wmo code",
            "temperature_2m_max": "°C",
            "temperature_2m_min": "°C"
          },
          "daily": {
            "time": ["2026-06-23","2026-06-24","2026-06-25","2026-06-26","2026-06-27","2026-06-28","2026-06-29"],
            "weather_code": [2,3,61,0,80,95,4],
            "temperature_2m_max": [28.1,27.0,22.5,30.2,26.6,24.0,29.9],
            "temperature_2m_min": [17.2,16.8,15.0,18.1,16.0,15.5,17.7]
          }
        }
        """;

    public const string ForecastError =
        """
        { "error": true, "reason": "Latitude must be in range of -90 to 90°. Given: 1000.0." }
        """;
}
