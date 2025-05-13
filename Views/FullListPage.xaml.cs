//------------------------------------------------------------------------------
// N�zev souboru: FullListPage.xaml.cs
// Autor: Jan Nechanick�
// Popis: Tento soubor obsahuje logiku pro str�nku zobrazuj�c� kompletn� seznam z�vodn�k�.
// Datum vytvo�en�: 1.4.2025
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
            _timeOffset = Preferences.Get("TimeOffset", 0); // Na�ten� posunu startovn�ho �asu podle koridoru
            _ = LoadAllRunnersAsync(); // Explicitn� ignorujeme �lohu, aby nedo�lo k varov�n�
            UpdateTimeLoop();

            // Registrace zpr�vy � pouze jednou v konstruktoru  
            WeakReferenceMessenger.Default.Register<RunnerUpdatedMessage>(this, (r, message) =>
            {
                var updated = message.Runner;

                // Zkus�me naj�t z�vodn�ka ve skupin�ch  
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

                        runner.OnPropertyChanged(null); // Obnov� zobrazen�  
                        return;
                    }
                }

                // Pokud z�vodn�k ve skupin�ch nen� � p�id�me ho jako nov�  
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
        /// Aktualizuje �as na str�nce v pravideln�ch intervalech.
        /// </summary>
        private async void UpdateTimeLoop()
        {
            while (_isRunning)
            {
                // Po��t�n� aktu�ln�ho �asu s posunem zadan�m v nastaven�
                var adjustedTime = DateTime.Now.AddMinutes(_timeOffset);
                TimeLabelControl.Text = adjustedTime.ToString("HH:mm:ss");
                await Task.Delay(500);
            }
        }

        /// <summary>
        /// Zpracuje ud�lost p�i opu�t�n� str�nky.
        /// </summary>
        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _isRunning = false;
            WeakReferenceMessenger.Default.Unregister<RunnerUpdatedMessage>(this);
        }

        /// <summary>
        /// Zpracuje ud�lost p�i n�vratu na str�nku.
        /// </summary>
        protected override void OnNavigatedTo(NavigatedToEventArgs args)
        {
            base.OnNavigatedTo(args);
            _isRunning = true;
            UpdateTimeLoop();
            _ = LoadAllRunnersAsync();
        }

        /// <summary>
        /// Zpracuje kliknut� na z�vodn�ka v seznamu.
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
        /// Otev�e str�nku pro �pravu vybran�ho z�vodn�ka.
        /// </summary>
        private async void OnEditRunnerClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.BindingContext is Runner selectedRunner)
            {
                await Navigation.PushAsync(new RunnerDetailPage(selectedRunner, _httpClient, _raceDataService, _runnerDatabase));
            }
        }

        /// <summary>
        /// P�id� nov�ho z�vodn�ka a otev�e str�nku pro jeho �pravu.
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
        /// Synchronizuje data se serverem a aktualizuje zobrazen�.
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
        /// Zpracuje zm�nu textu ve vyhled�vac�m poli a filtruje seznam z�vodn�k�.
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
        /// Vyma�e text ve vyhled�vac�m poli a obnov� seznam z�vodn�k�.
        /// </summary>
        private void OnClearSearchClicked(object sender, EventArgs e)
        {
            SearchEntry.Text = string.Empty;
            _ = LoadAllRunnersAsync();
        }
    }
}