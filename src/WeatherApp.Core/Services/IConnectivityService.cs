namespace WeatherApp.Core.Services;

public interface IConnectivityService
{
    bool IsConnected { get; }

    event EventHandler? ConnectivityChanged;
}
