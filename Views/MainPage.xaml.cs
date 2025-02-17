using Microsoft.Maui.Controls;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using StartCheckerApp.Models;
using StartCheckerApp;

namespace StartCheckerApp.Views
{
    public partial class MainPage : ContentPage
    {
        private readonly IServiceProvider _serviceProvider;

        public MainPage(IServiceProvider serviceProvider)
        {
            InitializeComponent();
            _serviceProvider = serviceProvider;
        }

        private async void OnNavigateToGetStartlist(object sender, EventArgs e)
        {
            var getStartlistPage = _serviceProvider.GetRequiredService<GetStartlistPage>();
            await Navigation.PushAsync(getStartlistPage);
        }

        private async void OnNavigateToFullList(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new FullListPage());
        }

        private async void OnNavigateToCurrentMinute(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new CurrentMinutePage());
        }

        private async void OnNavigateToSettings(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new SettingsPage());
        }

    }
}
