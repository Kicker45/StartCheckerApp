using StartCheckerApp.Models;
using CommunityToolkit.Maui.Views;
using System.ComponentModel;
using StartCheckerApp.Services;
using System.Collections.ObjectModel;

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
                selectedRunner.StartFlag = !selectedRunner.StartFlag;
                selectedRunner.Status = selectedRunner.StartFlag ? "Started" : "None";
                selectedRunner.StartPassage = selectedRunner.StartFlag ? DateTime.UtcNow : null;
                selectedRunner.LastModified = DateTime.UtcNow; // Pro offline synchronizaci

                // Uložit zmìnu do SQLite
                await _runnerDatabase.UpdateRunnerAsync(selectedRunner);

                // Najdeme správnou skupinu ve `GroupedRunners`
                if (RunnersList.ItemsSource is ObservableCollection<GroupedRunners> groupedRunners)
                {
                    var group = groupedRunners.FirstOrDefault(g => g.StartTime == selectedRunner.StartTime.ToString("d MMMM HH:mm:ss"));
                    if (group != null)
                    {
                        // Najdeme závodníka ve skupinì
                        var index = group.IndexOf(group.FirstOrDefault(r => r.ID == selectedRunner.ID));
                        if (index >= 0)
                        {
                            group[index] = selectedRunner; // Aktualizujeme pouze vybraného závodníka
                        }
                    }
                }
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
                StartTime = DateTime.UtcNow,
                StartPassage = null,
                Category = "",
                Status = "DNS",
                RaceId = _raceDataService.RaceId
            };

            await HandleRunnerPopup(newRunner);
        }

        private async void OnSyncClicked(object sender, EventArgs e)
        {
            LoadingIndicator.IsVisible = true;
            LoadingIndicator.IsRunning = true;

            bool isServerAvailable = await CheckServerAvailability();

            if (!isServerAvailable)
            {
                await DisplayAlert("Synchronizace", "Nepodaøilo se navázat spojení se serverem. Zmìny budou uchovány lokálnì.", "OK");
                LoadingIndicator.IsRunning = false;
                LoadingIndicator.IsVisible = false;
                return;
            }

            var modifiedRunners = (await _runnerDatabase.GetRunnersAsync())
                .Where(r => r.LastModified > _raceDataService.LastSyncTime)
                .ToList();

            if (modifiedRunners.Count == 0)
            {
                await DisplayAlert("Synchronizace", "Žádné nové zmìny k odeslání.", "OK");
                LoadingIndicator.IsRunning = false;
                LoadingIndicator.IsVisible = false;
                return;
            }

            foreach (var runner in modifiedRunners)
            {
                await _raceDataService.UpdateRunnerOnServer(runner);
            }

            _raceDataService.LastSyncTime = DateTime.UtcNow;
            await DisplayAlert("Synchronizace", "Všechny zmìny byly odeslány na server.", "OK");

            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
        }



        private async Task<bool> CheckServerAvailability()
        {
            try
            {
                string url = $"get-startlist?raceId=1";
                HttpResponseMessage response = await _httpClient.GetAsync(url);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false; // Pokud dojde k chybì, server není dostupný
            }
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
                200 => "Úspìch: Závodník byl úspìšnì aktualizován/pøidán.",
                400 => "Chyba: Neplatná data. Zkontrolujte zadané údaje.",
                404 => "Chyba: Závodník nebyl nalezen.",
                409 => "Chyba: Závodník s tímto ID již existuje.",
                _ => "Chyba: Nepodaøilo se dokonèit operaci."
            };

            await DisplayAlert(responseCode == 200 ? "Úspìch" : "Chyba", message, "OK");
        }
    }
}
