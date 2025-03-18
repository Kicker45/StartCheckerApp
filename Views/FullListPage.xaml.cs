using StartCheckerApp.Models;
using CommunityToolkit.Maui.Views;
using System.ComponentModel;
using StartCheckerApp.Services;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text;

namespace StartCheckerApp.Views
{
    public partial class FullListPage : ContentPage, INotifyPropertyChanged
    {
        private readonly RunnerDatabaseService _runnerDatabase;
        private readonly RaceDataService _raceDataService;
        private readonly HttpClient _httpClient;
        private bool _isRunning = true;

        public FullListPage(RaceDataService raceDataService, HttpClient httpClient, RunnerDatabaseService runnerDatabase)
        {
            InitializeComponent();
            _runnerDatabase = runnerDatabase;
            _raceDataService = raceDataService;
            _httpClient = httpClient;
            LoadRunners();

            // Naètení závodníkù ze služby
            RunnersList.ItemsSource = _raceDataService.Runners;

            // Synchronizace èasu
            UpdateTimeLoop();
        }

        private async void UpdateTimeLoop()
        {
            while (_isRunning)
            {
                TimeLabel.Text = DateTime.Now.ToString("HH:mm:ss");
                await Task.Delay(500); // Aktualizace každou sekundu
            }
        }

        private async void LoadRunners()
        {
            var runners = await _runnerDatabase.GetRunnersAsync();

            var groupedRunners = new ObservableCollection<GroupedRunners>(
                runners.OrderBy(r => r.StartTime)
                       .GroupBy(r => r.StartTime.ToString("d MMMM HH:mm:ss"))
                       .Select(g => new GroupedRunners(g.Key, g.ToList()))
            );

            RunnersList.ItemsSource = groupedRunners;
        }

        public class GroupedRunners : List<Runner>
        {
            public string StartTime { get; }

            public GroupedRunners(string startTime, List<Runner> runners) : base(runners)
            {
                StartTime = startTime;
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _isRunning = false; // Zastavíme aktualizaci èasu, když uživatel opustí stránku
        }

        private async void OnRunnerClicked(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.Count == 0)
                return;

            var selectedRunner = (Runner)e.CurrentSelection.FirstOrDefault();
            if (selectedRunner != null)
            {
                if (selectedRunner.Started == false)
                {
                    selectedRunner.StartFlag = true;
                    selectedRunner.Started = true;
                    selectedRunner.DNS = false;
                    selectedRunner.StartPassage = DateTime.UtcNow;
                    selectedRunner.LastUpdatedAt = DateTime.UtcNow;
                }
                else
                {
                    bool answer = await DisplayAlert(
                    "Upravit stav závodníka",
                    "Tento závodník je oznaèen jako odstartovaný. Chcete ho oznaèit jako neodstartovaný?",
                    "Ano",
                    "Ne");

                    if (answer)
                    {

                        selectedRunner.Started = false;
                        selectedRunner.DNS = false;
                        selectedRunner.StartPassage = null;
                        selectedRunner.StartFlag = false;
                        selectedRunner.LastUpdatedAt = DateTime.UtcNow;
                    }
                }

                // Najdeme správnou skupinu ve `GroupedRunners`
                if (RunnersList.ItemsSource is ObservableCollection<GroupedRunners> groupedRunners)
                {
                    var group = groupedRunners.FirstOrDefault(g => g.StartTime == selectedRunner.StartTime.ToString("HH:mm"));

                    if (group != null)
                    {
                        int index = group.IndexOf(selectedRunner);
                        if (index >= 0)
                        {
                            // Odebereme a znovu vložíme runnera, èímž donutíme UI k aktualizaci
                            group.RemoveAt(index);
                            group.Insert(index, selectedRunner);
                        }
                    }

                }
                // Uložit zmìnu do SQLite
                await _runnerDatabase.UpdateRunnerAsync(selectedRunner);
            }

        ((CollectionView)sender).SelectedItem = null; // Zruší výbìr po kliknutí
        }


        private async void OnEditRunnerClicked(object sender, EventArgs e)
        {
            var button = sender as Button;
            if (button?.BindingContext is Runner selectedRunner)
            {
                await HandleRunnerPopup(selectedRunner);
            }
        }


        private async void OnAddRunnerClicked(object sender, EventArgs e)
        {
            var newRunner = new Runner
            {
                RegistrationNumber = "",
                FirstName = "",
                Surname = "",
                SINumber = 0,
                StartTime = DateTime.UtcNow, //TODO potøeba zmìnit
                StartPassage = null,
                Category = "",
                DNS = false,
                Started = false,
                RaceId = _raceDataService.RaceId
            };

            await HandleRunnerPopup(newRunner);
        }

        private async void OnSyncClicked(object sender, EventArgs e)
        {
            LoadingIndicator.IsVisible = true;
            LoadingIndicator.IsRunning = true;

            await _raceDataService.SyncRunnersWithServer();

            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;

            // Znovu naèteme startovní listinu z lokální SQLite databáze
            LoadRunners();
        }



        private async Task HandleRunnerPopup(Runner runner)
        {
            var popup = new RunnerDetailPopup(runner, _httpClient, _raceDataService, _runnerDatabase);
            var responseCode = await this.ShowPopupAsync(popup) as int?;

            if (responseCode.HasValue)
            {
                ShowResponseAlert(responseCode.Value);
                if (responseCode == 200)
                {
                    LoadRunners();
                }
            }
        }



        private async void ShowResponseAlert(int responseCode)
        {
            string message = responseCode switch
            {
                200 => "Úspìch: Závodník byl úspìšnì aktualizován/pøidán do lokální databáze. Pro synchronizaci se serverem kliknìte na SYNC.",
                400 => "Chyba: Neplatná data. Zkontrolujte zadané údaje.",
                404 => "Chyba: Závodník nebyl nalezen.",
                409 => "Chyba: Závodník s tímto ID již existuje.",
                _ => "Chyba: Nepodaøilo se dokonèit operaci."
            };

            await DisplayAlert(responseCode == 200 ? "Úspìch" : "Chyba", message, "OK");
        }
    }
}
