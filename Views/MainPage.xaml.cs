using StartCheckerApp.Services;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;

namespace StartCheckerApp.Views
{
    public partial class MainPage : ContentPage
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly RaceDataService _raceDataService;
        private readonly RunnerDatabaseService _runnerDatabase;
        private readonly IMessageService _messageService;

        public MainPage(IServiceProvider serviceProvider, RaceDataService raceDataService, RunnerDatabaseService runnerDatabase, IMessageService messageService)
        {
            InitializeComponent();
            _serviceProvider = serviceProvider;
            _raceDataService = raceDataService;
            _runnerDatabase = runnerDatabase;
            _messageService = messageService;
        }

        private async void OnNavigateToGetStartlist(object sender, EventArgs e)
        {
            var getStartlistPage = _serviceProvider.GetRequiredService<GetStartlistPage>();
            await Navigation.PushAsync(getStartlistPage);
        }

        private async void OnNavigateToFullList(object sender, EventArgs e)
        {
            var runners = await _runnerDatabase.GetRunnersAsync();

            if (runners.Count == 0)
            {
                await _messageService.ShowMessageAsync("Nejdříve načti startovní listinu.");
                return;
            }

            var fullListPage = _serviceProvider.GetRequiredService<FullListPage>();
            await Navigation.PushAsync(fullListPage);
        }

        private async void OnNavigateToCurrentMinute(object sender, EventArgs e)
        {
            var runners = await _runnerDatabase.GetRunnersAsync();

            if (runners.Count == 0)
            {
                await _messageService.ShowMessageAsync("Nejdříve načti startovní listinu.");
                return;
            }

            var currentMinutePage = _serviceProvider.GetRequiredService<CurrentMinutePage>();
            await Navigation.PushAsync(currentMinutePage);
        }

        private async void OnNavigateToSettings(object sender, EventArgs e)
        {
            //var siService = _serviceProvider.GetRequiredService<UsbCommunicationService>();
            //await Navigation.PushAsync(new SettingsPage(siService));
            var settingsPage = _serviceProvider.GetRequiredService<SettingsPage>();
            await Navigation.PushAsync(settingsPage);
        }
    }
}
