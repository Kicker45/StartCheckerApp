using StartCheckerApp.Models;
using StartCheckerApp.Services;
using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Views;


namespace StartCheckerApp.Views
{
    public abstract class BaseRunnersPage : ContentPage
    {
        protected readonly RunnerDatabaseService _runnerDatabase;
        protected readonly RaceDataService _raceDataService;
        protected readonly HttpClient _httpClient;

        protected ObservableCollection<RunnerGroup> RunnerGroups = new();

        public BaseRunnersPage(RaceDataService raceDataService, HttpClient httpClient, RunnerDatabaseService runnerDatabase)
        {
            _raceDataService = raceDataService;
            _httpClient = httpClient;
            _runnerDatabase = runnerDatabase;
        }

        protected abstract CollectionView RunnersCollectionView { get; }
        protected abstract Label TimeLabel { get; }
        protected RunnerDatabaseService RunnerDatabase => _runnerDatabase;

        protected async Task LoadAllRunnersAsync(Func<Runner, bool> filter = null)
        {
            var allRunners = await Task.Run(() => _runnerDatabase.GetRunnersAsync().Result);

            var filtered = filter != null ? allRunners.Where(filter).ToList() : allRunners;

            var grouped = await Task.Run(() =>
                filtered.OrderBy(r => r.StartTime.ToLocalTime())
                        .GroupBy(r => r.StartMinute)
                        .Select(g => new RunnerGroup(g.Key, g))
                        .ToList());

            MainThread.BeginInvokeOnMainThread(() =>
            {
                RunnerGroups = new ObservableCollection<RunnerGroup>(grouped);
                RunnersCollectionView.ItemsSource = RunnerGroups;
            });
        }


        protected async Task HandleRunnerClickAsync(Runner selectedRunner)
        {
            if (selectedRunner == null) return;

            if (!selectedRunner.DNS)
            {
                if (!selectedRunner.Started)
                {
                    selectedRunner.StartFlag = true;
                    selectedRunner.Started = true;
                    selectedRunner.StartPassage = DateTime.UtcNow;
                }
                else
                {
                    bool undo = await DisplayAlert("Změna stavu", "Závodník je označen jako odstartovaný. Vrátit zpět?", "Ano", "Ne");
                    if (undo)
                    {
                        selectedRunner.Started = false;
                        selectedRunner.StartFlag = false;
                        selectedRunner.StartPassage = null;
                    }
                }
            }
            else
            {
                bool confirm = await DisplayAlert("Změna stavu", "Závodník má stav DNS. Označit jako odstartovaný?", "Ano", "Ne");
                if (confirm)
                {
                    selectedRunner.Started = true;
                    selectedRunner.StartFlag = true;
                    selectedRunner.DNS = false;
                    selectedRunner.StartPassage = DateTime.UtcNow;
                }
            }

            selectedRunner.LastUpdatedAt = DateTime.UtcNow;
            await _runnerDatabase.UpdateRunnerAsync(selectedRunner);
        }

        protected async Task HandleRunnerDetailPageAsync(Runner runner)
        {
            await Navigation.PushAsync(new RunnerDetailPage(runner, _httpClient, _raceDataService, _runnerDatabase));
        }


        protected async Task SyncWithServerAsync()
        {
            await _raceDataService.SyncRunnersWithServer();
            await LoadAllRunnersAsync();
        }
    }
}
