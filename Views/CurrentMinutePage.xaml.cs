using StartCheckerApp.Models;
using CommunityToolkit.Maui.Views;
using System.Collections.ObjectModel;
using StartCheckerApp.Services;

namespace StartCheckerApp.Views
{
    public partial class CurrentMinutePage : BaseRunnersPage
    {
        private readonly List<string> _minuteGroups = new();
        private ObservableCollection<RunnerGroup> _allGroupedRunners = new();
        private int _currentMinuteIndex = 0;
        private bool _isRunning = true;

        // Implementace TimeLabel pro tuto stránku
        protected override Label TimeLabel => TimeLabelControl;

        // Implementace CollectionView pro tuto stránku
        protected override CollectionView RunnersCollectionView => RunnersList;

        public CurrentMinutePage(RaceDataService raceDataService, HttpClient httpClient, RunnerDatabaseService runnerDatabase)
            : base(raceDataService, httpClient, runnerDatabase)
        {
            InitializeComponent();
            LoadGroupedRunners();
            SetupGestureNavigation();
            UpdateTimeLoop();
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
        }
        private async void LoadGroupedRunners()
        {
            var runners = await RunnerDatabase.GetRunnersAsync();

            _allGroupedRunners = [.. runners.OrderBy(r => r.StartTime)
                       .GroupBy(r => r.StartTime.ToString("HH:mm"))
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
            var button = sender as Button;
            if (button?.BindingContext is Runner selectedRunner)
            {
                await HandleRunnerPopupAsync(selectedRunner);
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

            await HandleRunnerPopupAsync(newRunner);
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

        private void SetupGestureNavigation()
        {
            // Zajistíme swipe gesta pro CollectionView
            var swipeLeft = new SwipeGestureRecognizer { Direction = SwipeDirection.Left };
            swipeLeft.Swiped += OnSwipeLeft;

            var swipeRight = new SwipeGestureRecognizer { Direction = SwipeDirection.Right };
            swipeRight.Swiped += OnSwipeRight;

            RunnersList.GestureRecognizers.Add(swipeLeft); // Pøiøadíme gesta pøímo k CollectionView
            RunnersList.GestureRecognizers.Add(swipeRight);
        }

        private void OnSwipeLeft(object sender, SwipedEventArgs e)
        {
            GoToNextMinute();
        }

        private void OnSwipeRight(object sender, SwipedEventArgs e)
        {
            GoToPreviousMinute();
        }

        private void GoToNextMinute()
        {
            if (_currentMinuteIndex < _minuteGroups.Count - 1)
            {
                _currentMinuteIndex++;
                SetCurrentMinuteGroup(_minuteGroups[_currentMinuteIndex]);
            }
        }

        private void GoToPreviousMinute()
        {
            if (_currentMinuteIndex > 0)
            {
                _currentMinuteIndex--;
                SetCurrentMinuteGroup(_minuteGroups[_currentMinuteIndex]);
            }
        }


    }
}
