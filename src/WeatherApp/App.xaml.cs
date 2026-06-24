using WeatherApp.Core.Services;

namespace WeatherApp;

public partial class App : Application
{
	public App(IThemeService theme)
	{
		InitializeComponent();

		theme.Apply();
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		return new Window(new AppShell());
	}
}