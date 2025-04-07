using StartCheckerApp.Services;

namespace StartCheckerApp.Views
{
    public partial class MainPage : ContentPage
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly RaceDataService _raceDataService;
        private readonly RunnerDatabaseService _runnerDatabase;

        public MainPage(IServiceProvider serviceProvider, RaceDataService raceDataService, RunnerDatabaseService runnerDatabase)
        {
            InitializeComponent();
            _serviceProvider = serviceProvider;
            _raceDataService = raceDataService;
            _runnerDatabase = runnerDatabase;
            
        }

        private async void OnNavigateToGetStartlist(object sender, EventArgs e)
        {
            var getStartlistPage = _serviceProvider.GetRequiredService<GetStartlistPage>();
            await Navigation.PushAsync(getStartlistPage);
        }

        private async void OnNavigateToFullList(object sender, EventArgs e)
        {
            var runners = await _runnerDatabase.GetRunnersAsync(); // Načteme závodníky z SQLite

            if (runners.Count == 0)
            {
                await DisplayAlert("Chyba", "Nejdříve načti startovní listinu.", "OK");
                return;
            }

            var fullListPage = _serviceProvider.GetRequiredService<FullListPage>();
            await Navigation.PushAsync(fullListPage);
        }



        private async void OnNavigateToCurrentMinute(object sender, EventArgs e)
        {
            var runners = await _runnerDatabase.GetRunnersAsync(); // Načteme závodníky z SQLite

            if (runners.Count == 0)
            {
                await DisplayAlert("Chyba", "Nejdříve načti startovní listinu.", "OK");
                return;
            }
            var currentMinutePage = _serviceProvider.GetRequiredService<CurrentMinutePage>();
            await Navigation.PushAsync(currentMinutePage);
        }

        private async void OnNavigateToSettings(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new SettingsPage());
        }

    }
}
