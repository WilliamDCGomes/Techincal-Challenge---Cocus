using WeatherApp.Core.Enums;

namespace WeatherApp.Core.Services;

public sealed class WeatherException : Exception
{
    public WeatherException(ErrorKind kind, string message, Exception? innerException = null)
        : base(message, innerException)
    {
        Kind = kind;
    }

    public ErrorKind Kind { get; }
}
