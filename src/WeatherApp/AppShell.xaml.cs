using WeatherApp.Views;

namespace WeatherApp;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute(Routes.Results, typeof(ResultsPage));
    }
}
