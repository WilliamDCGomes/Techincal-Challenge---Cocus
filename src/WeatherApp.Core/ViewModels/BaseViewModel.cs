using CommunityToolkit.Mvvm.ComponentModel;
using WeatherApp.Core.Enums;
using WeatherApp.Core.Services;

namespace WeatherApp.Core.ViewModels;

public abstract partial class BaseViewModel : ObservableObject
{
    [ObservableProperty]
    public partial ViewState State { get; set; }

    [ObservableProperty]
    public partial ErrorKind ErrorKind { get; set; }

    [ObservableProperty]
    public partial string? ErrorMessage { get; set; }

    [ObservableProperty]
    public partial bool IsBusy { get; set; }

    protected Task RunGuardedAsync(
        Func<CancellationToken, Task<ViewState>> operation,
        CancellationToken cancellationToken = default) =>
        RunGuardedAsync(operation, showLoading: true, cancellationToken);

    protected async Task RunGuardedAsync(
        Func<CancellationToken, Task<ViewState>> operation,
        bool showLoading,
        CancellationToken cancellationToken = default)
    {
        if (IsBusy)
        {
            return;
        }

        IsBusy = true;
        ClearError();
        if (showLoading)
        {
            State = ViewState.Loading;
        }

        try
        {
            State = await operation(cancellationToken);
        }
        catch (OperationCanceledException)
        {
        }
        catch (WeatherException ex)
        {
            SetError(ex.Kind);
        }
        catch (HttpRequestException)
        {
            SetError(Enums.ErrorKind.Network);
        }
        catch (Exception)
        {
            SetError(Enums.ErrorKind.Unknown);
        }
        finally
        {
            IsBusy = false;
        }
    }

    protected void SetError(ErrorKind kind)
    {
        ErrorKind = kind;
        ErrorMessage = ErrorMessages.For(kind);
        State = ViewState.Error;
    }

    protected void ResetToIdle()
    {
        ClearError();
        State = ViewState.Idle;
    }

    private void ClearError()
    {
        ErrorKind = Enums.ErrorKind.None;
        ErrorMessage = null;
    }

    partial void OnIsBusyChanged(bool value) => OnBusyChanged();

    protected virtual void OnBusyChanged()
    {
    }
}
