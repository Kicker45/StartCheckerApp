using StartCheckerApp.Models;
using CommunityToolkit.Maui.Views;
using System.Collections.ObjectModel;
using StartCheckerApp.Services;
using StartCheckerApp.Messages;
using CommunityToolkit.Mvvm.Messaging;

namespace StartCheckerApp.Views
{
    public partial class CurrentMinutePage : BaseRunnersPage
    {
        private readonly List<string> _minuteGroups = new();
        private ObservableCollection<RunnerGroup> _allGroupedRunners = new();
        private int _currentMinuteIndex = 0;
        private bool _isRunning = true;

        protected override Label TimeLabel => TimeLabelControl;
        protected override CollectionView RunnersCollectionView => RunnersList;

        public CurrentMinutePage(RaceDataService raceDataService, HttpClient httpClient, RunnerDatabaseService runnerDatabase)
            : base(raceDataService, httpClient, runnerDatabase)
        {
            InitializeComponent();
            LoadGroupedRunners();
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
                var groupKey = updated.StartTime.ToString("HH:mm");
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

        private async void UpdateTimeLoop()
        {
            while (_isRunning)
            {
                TimeLabelControl.Text = DateTime.Now.ToString("HH:mm:ss");
                await Task.Delay(500);
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _isRunning = false;
            WeakReferenceMessenger.Default.Unregister<RunnerUpdatedMessage>(this);
        }
        protected override void OnNavigatedTo(NavigatedToEventArgs args)
        {
            base.OnNavigatedTo(args);
            _isRunning = true;
            UpdateTimeLoop();

        }

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

        private void UpdateToCurrentMinute()
        {
            var currentMinute = DateTime.Now.ToString("HH:mm");
            SetCurrentMinuteGroup(currentMinute);
        }

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

        private async void OnEditRunnerClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.BindingContext is Runner selectedRunner)
            {
                await Navigation.PushAsync(new RunnerDetailPage(selectedRunner, _httpClient, _raceDataService, _runnerDatabase));
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
                DNS = false,
                Started = false,
                RaceId = _raceDataService.RaceId
            };

            await Navigation.PushAsync(new RunnerDetailPage(newRunner, _httpClient, _raceDataService, _runnerDatabase));
        }

        private async void OnSyncClicked(object sender, EventArgs e)
        {
            LoadingIndicator.IsVisible = true;
            LoadingIndicator.IsRunning = true;

            await SyncWithServerAsync();

            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;

            UpdateToCurrentMinute();
        }

        private void OnPreviousMinuteClicked(object sender, EventArgs e)
        {
            if (_currentMinuteIndex > 0)
            {
                _currentMinuteIndex--;
                SetCurrentMinuteGroup(_minuteGroups[_currentMinuteIndex]);
            }
        }

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
