using Microsoft.Maui.Networking;
using WeatherApp.Core.Services;

namespace WeatherApp.Services;

public sealed class ConnectivityService : IConnectivityService
{
    private readonly IConnectivity _connectivity;

    public ConnectivityService(IConnectivity connectivity)
    {
        _connectivity = connectivity;
        _connectivity.ConnectivityChanged += OnPlatformConnectivityChanged;
    }

    private void OnPlatformConnectivityChanged(object? sender, ConnectivityChangedEventArgs e) =>
        MainThread.BeginInvokeOnMainThread(() => ConnectivityChanged?.Invoke(this, EventArgs.Empty));

    public bool IsConnected => _connectivity.NetworkAccess == NetworkAccess.Internet;

    public event EventHandler? ConnectivityChanged;
}
