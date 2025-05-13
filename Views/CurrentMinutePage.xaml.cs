//------------------------------------------------------------------------------
// N�zev souboru: CurrentMinutePage.xaml.cs
// Autor: Jan Nechanick�
// Popis: Tento soubor obsahuje logiku pro str�nku zobrazuj�c� aktu�ln� startovn� minutu.
// Datum vytvo�en�: 1.4.2025
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
            _timeOffset = Preferences.Get("TimeOffset", 0); // Na�ten� posunu startovn�ho �asu podle koridoru
            UpdateTimeLoop();

            // Registrace zpr�vy pouze jednou
            WeakReferenceMessenger.Default.Register<RunnerUpdatedMessage>(this, (r, message) =>
            {
                var updated = message.Runner;

                // Zkusit naj�t a aktualizovat z�vodn�ka ve v�ech skupin�ch
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

                // Pokud z�vodn�k neexistuje � p�id�me ho do p��slu�n� minuty
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
        }

        /// <summary>
        /// Na�te z�vodn�ky z datab�ze a seskup� je podle startovn� minuty.
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
        /// Aktualizuje zobrazen� na aktu�ln� startovn� minutu.
        /// </summary>
        private void UpdateToCurrentMinute()
        {
            var currentMinute = DateTime.Now.AddMinutes(_timeOffset).ToString("HH:mm");
            SetCurrentMinuteGroup(currentMinute);
        }

        /// <summary>
        /// Nastav� skupinu z�vodn�k� pro zobrazen� podle zadan� minuty.
        /// </summary>
        /// <param name="minute">Startovn� minuta.</param>
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
        /// P�id� nov�ho z�vodn�ka s pr�zdn�mi hodnotami a otev�e str�nku pro jeho �pravu.
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

            UpdateToCurrentMinute();
        }

        /// <summary>
        /// P�epne na p�edchoz� startovn� minutu.
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
        /// P�epne na n�sleduj�c� startovn� minutu.
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
