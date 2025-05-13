//------------------------------------------------------------------------------
// Název souboru: FullListPage.xaml.cs
// Autor: Jan Nechanický
// Popis: Tento soubor obsahuje logiku pro stránku zobrazující kompletní seznam závodníkù.
// Datum vytvoøení: 1.4.2025
//------------------------------------------------------------------------------

using CommunityToolkit.Mvvm.Messaging;
using StartCheckerApp.Messages;
using StartCheckerApp.Models;
using StartCheckerApp.Services;
using System.Collections.ObjectModel;

namespace StartCheckerApp.Views
{
    public partial class FullListPage : BaseRunnersPage
    {
        private bool _isRunning = true;
        private int _timeOffset = 0;

        protected override Label TimeLabel => TimeLabelControl;
        protected override CollectionView RunnersCollectionView => RunnersList;

        public FullListPage(RaceDataService raceDataService, HttpClient httpClient, RunnerDatabaseService runnerDatabase)
           : base(raceDataService, httpClient, runnerDatabase)
        {
            InitializeComponent();
            _timeOffset = Preferences.Get("TimeOffset", 0); // Naètení posunu startovního èasu podle koridoru
            _ = LoadAllRunnersAsync(); // Explicitnì ignorujeme úlohu, aby nedošlo k varování
            UpdateTimeLoop();

            // Registrace zprávy – pouze jednou v konstruktoru  
            WeakReferenceMessenger.Default.Register<RunnerUpdatedMessage>(this, (r, message) =>
            {
                var updated = message.Runner;

                // Zkusíme najít závodníka ve skupinách  
                foreach (var group in RunnerGroups)
                {
                    var runner = group.FirstOrDefault(r => r.ID == updated.ID);
                    if (runner != null)
                    {
                        runner.FirstName = updated.FirstName;
                        runner.Surname = updated.Surname;
                        runner.Category = updated.Category;
                        runner.RegistrationNumber = updated.RegistrationNumber;
                        runner.SINumber = updated.SINumber;
                        runner.StartTime = updated.StartTime;
                        runner.DNS = updated.DNS;
                        runner.Started = updated.Started;
                        runner.StartFlag = updated.StartFlag;
                        runner.StartPassage = updated.StartPassage;
                        runner.LastUpdatedAt = updated.LastUpdatedAt;

                        runner.OnPropertyChanged(null); // Obnoví zobrazení  
                        return;
                    }
                }

                // Pokud závodník ve skupinách není – pøidáme ho jako nový  
                var key = updated.StartMinute;
                var existingGroup = RunnerGroups.FirstOrDefault(g => g.StartTime == key);

                if (existingGroup != null)
                {
                    existingGroup.Add(updated);
                }
                else
                {
                    RunnerGroups.Add(new RunnerGroup(key, new List<Runner> { updated }));
                }
            });
        }

        /// <summary>
        /// Aktualizuje èas na stránce v pravidelných intervalech.
        /// </summary>
        private async void UpdateTimeLoop()
        {
            while (_isRunning)
            {
                // Poèítání aktuálního èasu s posunem zadaným v nastavení
                var adjustedTime = DateTime.Now.AddMinutes(_timeOffset);
                TimeLabelControl.Text = adjustedTime.ToString("HH:mm:ss");
                await Task.Delay(500);
            }
        }

        /// <summary>
        /// Zpracuje událost pøi opuštìní stránky.
        /// </summary>
        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _isRunning = false;
            WeakReferenceMessenger.Default.Unregister<RunnerUpdatedMessage>(this);
        }

        /// <summary>
        /// Zpracuje událost pøi návratu na stránku.
        /// </summary>
        protected override void OnNavigatedTo(NavigatedToEventArgs args)
        {
            base.OnNavigatedTo(args);
            _isRunning = true;
            UpdateTimeLoop();
            _ = LoadAllRunnersAsync();
        }

        /// <summary>
        /// Zpracuje kliknutí na závodníka v seznamu.
        /// </summary>
        private async void OnRunnerClicked(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.Count == 0)
                return;

            var selectedRunner = (Runner)e.CurrentSelection.FirstOrDefault();
            if (selectedRunner != null)
            {
                await HandleRunnerClickAsync(selectedRunner);
            }

            ((CollectionView)sender).SelectedItem = null;
        }

        /// <summary>
        /// Otevøe stránku pro úpravu vybraného závodníka.
        /// </summary>
        private async void OnEditRunnerClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.BindingContext is Runner selectedRunner)
            {
                await Navigation.PushAsync(new RunnerDetailPage(selectedRunner, _httpClient, _raceDataService, _runnerDatabase));
            }
        }

        /// <summary>
        /// Pøidá nového závodníka a otevøe stránku pro jeho úpravu.
        /// </summary>
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
                DNS = false,
                Started = false,
                RaceId = _raceDataService.RaceId
            };

            await Navigation.PushAsync(new RunnerDetailPage(newRunner, _httpClient, _raceDataService, _runnerDatabase));
        }

        /// <summary>
        /// Synchronizuje data se serverem a aktualizuje zobrazení.
        /// </summary>
        private async void OnSyncClicked(object sender, EventArgs e)
        {
            LoadingIndicator.IsVisible = true;
            LoadingIndicator.IsRunning = true;

            await SyncWithServerAsync();

            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
        }

        /// <summary>
        /// Zpracuje zmìnu textu ve vyhledávacím poli a filtruje seznam závodníkù.
        /// </summary>
        private async void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            string query = e.NewTextValue?.ToLower().Trim();

            var runners = await _runnerDatabase.GetRunnersAsync();

            var filtered = runners
                .Where(r =>
                    (r.FirstName?.ToLower().Contains(query) ?? false) ||
                    (r.Surname?.ToLower().Contains(query) ?? false) ||
                    (r.FullName?.ToLower().Contains(query) ?? false) ||
                    (r.Category?.ToLower().Contains(query) ?? false) ||
                    (r.RegistrationNumber?.ToLower().Contains(query) ?? false) ||
                    r.SINumber.ToString().Contains(query) ||
                    r.StartTime.ToLocalTime().ToString("HH:mm:ss").Contains(query))
                .ToList();

            var grouped = filtered
                .OrderBy(r => r.StartTime)
                .GroupBy(r => r.StartMinute)
                .Select(g => new RunnerGroup(g.Key, g))
                .ToList();

            RunnersList.ItemsSource = new ObservableCollection<RunnerGroup>(grouped);
        }

        /// <summary>
        /// Vymaže text ve vyhledávacím poli a obnoví seznam závodníkù.
        /// </summary>
        private void OnClearSearchClicked(object sender, EventArgs e)
        {
            SearchEntry.Text = string.Empty;
            _ = LoadAllRunnersAsync();
        }
    }
}