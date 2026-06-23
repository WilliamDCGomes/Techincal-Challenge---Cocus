using WeatherApp.Core.ViewModels;

namespace WeatherApp.Views;

public partial class SearchPage : ContentPage
{
    public SearchViewModel ViewModel { get; }

    public SearchPage(SearchViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = ViewModel = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        _ = ViewModel.PrepareAsync();
    }
}
