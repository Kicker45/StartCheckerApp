//------------------------------------------------------------------------------
// Název souboru: BaseRunnersPage.cs
// Autor: Jan Nechanický
// Popis: Tento soubor obsahuje abstraktní základní třídu pro obě stránky zobrazující seznam závodníků (CurrentMinutePage, FullListPage).
// Datum vytvoření: 1.2.2025
//------------------------------------------------------------------------------
using StartCheckerApp.Models;
using StartCheckerApp.Services;
using System.Collections.ObjectModel;


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

        // Abstraktní vlastnost pro CollectionView, která bude implementována v odvozených třídách.
        protected abstract CollectionView RunnersCollectionView { get; }

        // Abstraktní vlastnost pro Label zobrazující čas, která bude implementována v odvozených třídách.
        protected abstract Label TimeLabel { get; }

        // Získání instance RunnerDatabaseService.
        protected RunnerDatabaseService RunnerDatabase => _runnerDatabase;

        /// <summary>
        /// Načte všechny závodníky z databáze, aplikuje volitelný filtr a seskupí je podle startovní minuty.
        /// </summary>
        /// <param name="filter">Volitelný filtr pro závodníky.</param>
        protected async Task LoadAllRunnersAsync(Func<Runner, bool> filter = null)
        {
            var allRunners = await Task.Run(() => _runnerDatabase.GetRunnersAsync().Result);

            var filtered = filter != null ? allRunners.Where(filter).ToList() : allRunners;

            var grouped = await Task.Run(() =>
                filtered.OrderBy(r => r.StartTime)
                        .GroupBy(r => r.StartMinute)
                        .Select(g => new RunnerGroup(g.Key, g))
                        .ToList());

            MainThread.BeginInvokeOnMainThread(() =>
            {
                RunnerGroups = new ObservableCollection<RunnerGroup>(grouped);
                RunnersCollectionView.ItemsSource = RunnerGroups;
            });
        }

        /// <summary>
        /// Zpracuje kliknutí na závodníka a umožní změnu jeho stavu (např. označení jako odstartovaný).
        /// </summary>
        /// <param name="selectedRunner">Vybraný závodník.</param>
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
            selectedRunner.OnPropertyChanged(nameof(Runner.ComputedStatus));
            await _runnerDatabase.UpdateRunnerAsync(selectedRunner);
        }

        /// <summary>
        /// Otevře detailní stránku závodníka pro úpravy nebo zobrazení.
        /// </summary>
        /// <param name="runner">Závodník, jehož detail se má zobrazit.</param>
        protected async Task HandleRunnerDetailPageAsync(Runner runner)
        {
            await Navigation.PushAsync(new RunnerDetailPage(runner, _httpClient, _raceDataService, _runnerDatabase));
        }

        /// <summary>
        /// Synchronizuje data závodníků se serverem a znovu načte seznam závodníků.
        /// </summary>
        protected async Task SyncWithServerAsync()
        {
            await _raceDataService.SyncRunnersWithServer();
            await LoadAllRunnersAsync();
        }
    }
}
