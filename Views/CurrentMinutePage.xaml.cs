//------------------------------------------------------------------------------
// Název souboru: CurrentMinutePage.xaml.cs
// Autor: Jan Nechanický
// Popis: Tento soubor obsahuje logiku pro stránku zobrazující aktuální startovní minutu.
// Datum vytvoøení: 1.4.2025
//------------------------------------------------------------------------------

using CommunityToolkit.Mvvm.Messaging;
using StartCheckerApp.Messages;
using StartCheckerApp.Models;
using StartCheckerApp.Services;
using System.Collections.ObjectModel;

namespace StartCheckerApp.Views
{
    public partial class CurrentMinutePage : BaseRunnersPage
    {
        private readonly List<string> _minuteGroups = new();
        private ObservableCollection<RunnerGroup> _allGroupedRunners = new();
        private int _currentMinuteIndex = 0;
        private bool _isRunning = true;
        private int _timeOffset = 0;

        protected override Label TimeLabel => TimeLabelControl;
        protected override CollectionView RunnersCollectionView => RunnersList;

        public CurrentMinutePage(RaceDataService raceDataService, HttpClient httpClient, RunnerDatabaseService runnerDatabase)
            : base(raceDataService, httpClient, runnerDatabase)
        {
            InitializeComponent();
            LoadGroupedRunners();
            _timeOffset = Preferences.Get("TimeOffset", 0); // Naètení posunu startovního èasu podle koridoru
            UpdateTimeLoop();

            // Registrace zprávy pouze jednou
            WeakReferenceMessenger.Default.Register<RunnerUpdatedMessage>(this, (r, message) =>
            {
                var updated = message.Runner;

                // Zkusit najít a aktualizovat závodníka ve všech skupinách
                foreach (var group in _allGroupedRunners)
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

                        runner.OnPropertyChanged(null);
                        UpdateToCurrentMinute();
                        return;
                    }
                }

                // Pokud závodník neexistuje – pøidáme ho do pøíslušné minuty
                var groupKey = updated.StartTime.ToLocalTime().ToString("HH:mm");
                var targetGroup = _allGroupedRunners.FirstOrDefault(g => g.StartTime == groupKey);

                if (targetGroup != null)
                {
                    targetGroup.Add(updated);
                }
                else
                {
                    _allGroupedRunners.Add(new RunnerGroup(groupKey, new List<Runner> { updated }));
                    _minuteGroups.Add(groupKey);
                }

                UpdateToCurrentMinute();
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
        }

        /// <summary>
        /// Naète závodníky z databáze a seskupí je podle startovní minuty.
        /// </summary>
        private async void LoadGroupedRunners()
        {
            var runners = await RunnerDatabase.GetRunnersAsync();

            _allGroupedRunners = [.. runners.OrderBy(r => r.StartTime)
                           .GroupBy(r => r.StartTime.ToLocalTime().ToString("HH:mm"))
                           .Select(g => new RunnerGroup(g.Key, g.ToList()))];

            _minuteGroups.Clear();
            foreach (var group in _allGroupedRunners)
            {
                _minuteGroups.Add(group.StartTime);
            }

            UpdateToCurrentMinute();
        }

        /// <summary>
        /// Aktualizuje zobrazení na aktuální startovní minutu.
        /// </summary>
        private void UpdateToCurrentMinute()
        {
            var currentMinute = DateTime.Now.AddMinutes(_timeOffset).ToString("HH:mm");
            SetCurrentMinuteGroup(currentMinute);
        }

        /// <summary>
        /// Nastaví skupinu závodníkù pro zobrazení podle zadané minuty.
        /// </summary>
        /// <param name="minute">Startovní minuta.</param>
        private void SetCurrentMinuteGroup(string minute)
        {
            _currentMinuteIndex = _minuteGroups.IndexOf(minute);
            if (_currentMinuteIndex < 0)
            {
                _currentMinuteIndex = 0;
            }

            if (_allGroupedRunners.Count > _currentMinuteIndex)
            {
                RunnersList.ItemsSource = new ObservableCollection<RunnerGroup>
                    {
                        _allGroupedRunners[_currentMinuteIndex]
                    };
            }
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
        /// Pøidá nového závodníka s prázdnými hodnotami a otevøe stránku pro jeho úpravu.
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

            UpdateToCurrentMinute();
        }

        /// <summary>
        /// Pøepne na pøedchozí startovní minutu.
        /// </summary>
        private void OnPreviousMinuteClicked(object sender, EventArgs e)
        {
            if (_currentMinuteIndex > 0)
            {
                _currentMinuteIndex--;
                SetCurrentMinuteGroup(_minuteGroups[_currentMinuteIndex]);
            }
        }

        /// <summary>
        /// Pøepne na následující startovní minutu.
        /// </summary>
        private void OnNextMinuteClicked(object sender, EventArgs e)
        {
            if (_currentMinuteIndex < _minuteGroups.Count - 1)
            {
                _currentMinuteIndex++;
                SetCurrentMinuteGroup(_minuteGroups[_currentMinuteIndex]);
            }
        }
    }
}
