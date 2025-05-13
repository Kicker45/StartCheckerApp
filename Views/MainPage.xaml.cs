//------------------------------------------------------------------------------
// Název souboru: MainPage.xaml.cs
// Autor: Jan Nechanický
// Popis: Tento soubor obsahuje obsluhu úvodní obrazovky a navigaci na další stránky.
// Datum vytvoření: 1.2.2025
//------------------------------------------------------------------------------

using StartCheckerApp.Services;

namespace StartCheckerApp.Views
{
    /// <summary>
    /// Hlavní stránka aplikace s navigačními tlačítky na různé sekce.
    /// </summary>
    public partial class MainPage : ContentPage
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly RaceDataService _raceDataService;
        private readonly RunnerDatabaseService _runnerDatabase;
        private readonly IMessageService _messageService;

        /// <summary>
        /// Konstruktor pro MainPage, kde se injektují potřebné služby.
        /// </summary>
        public MainPage(
            IServiceProvider serviceProvider,
            RaceDataService raceDataService,
            RunnerDatabaseService runnerDatabase,
            IMessageService messageService)
        {
            InitializeComponent();
            _serviceProvider = serviceProvider;
            _raceDataService = raceDataService;
            _runnerDatabase = runnerDatabase;
            _messageService = messageService;
        }

        /// <summary>
        /// Naviguje na stránku pro načtení startovní listiny ze serveru.
        /// </summary>
        private async void OnNavigateToGetStartlist(object sender, EventArgs e)
        {
            var getStartlistPage = _serviceProvider.GetRequiredService<GetStartlistPage>();
            await Navigation.PushAsync(getStartlistPage);
        }

        /// <summary>
        /// Naviguje na stránku se zobrazením úplného seznamu závodníků.
        /// </summary>
        private async void OnNavigateToFullList(object sender, EventArgs e)
        {
            var runners = await _runnerDatabase.GetRunnersAsync();
            var fullListPage = _serviceProvider.GetRequiredService<FullListPage>();
            await Navigation.PushAsync(fullListPage);
        }

        /// <summary>
        /// Naviguje na stránku pro zobrazení závodníků podle aktuální minuty;
        /// pokud není načtena startovní listina, zobrazí chybovou zprávu.
        /// </summary>
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

        /// <summary>
        /// Naviguje na stránku nastavení aplikace.
        /// </summary>
        private async void OnNavigateToSettings(object sender, EventArgs e)
        {
            var settingsPage = _serviceProvider.GetRequiredService<SettingsPage>();
            await Navigation.PushAsync(settingsPage);
        }
    }
}
