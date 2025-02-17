namespace StartCheckerApp.Views
{
    public partial class MainPage : ContentPage
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly RaceDataService _raceDataService;

        public MainPage(IServiceProvider serviceProvider, RaceDataService raceDataService)
        {
            InitializeComponent();
            _serviceProvider = serviceProvider;
            _raceDataService = raceDataService;
            
        }

        private async void OnNavigateToGetStartlist(object sender, EventArgs e)
        {
            var getStartlistPage = _serviceProvider.GetRequiredService<GetStartlistPage>();
            await Navigation.PushAsync(getStartlistPage);
        }

        private async void OnNavigateToFullList(object sender, EventArgs e)
        {
            if (_raceDataService.Runners.Count == 0)
            {
                await DisplayAlert("Chyba", "Nejdříve načti startovní listinu.", "OK");
                return;
            }
            var fullListPage = _serviceProvider.GetRequiredService<FullListPage>();
            await Navigation.PushAsync(fullListPage);
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
