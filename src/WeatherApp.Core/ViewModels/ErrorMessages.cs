using WeatherApp.Core.Enums;

namespace WeatherApp.Core.ViewModels;

internal static class ErrorMessages
{
    public static string For(ErrorKind kind) => kind switch
    {
        ErrorKind.Network => "Couldn't reach the weather service. Check your connection and try again.",
        ErrorKind.Offline => "You appear to be offline. Reconnect to load fresh weather.",
        ErrorKind.InvalidResponse => "The weather service returned an unexpected response. Please try again.",
        ErrorKind.Unknown => "Something went wrong. Please try again.",
        _ => string.Empty,
    };
}
