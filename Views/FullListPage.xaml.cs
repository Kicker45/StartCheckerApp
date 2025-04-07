using StartCheckerApp.Models;
using CommunityToolkit.Maui.Views;
using System.ComponentModel;
using StartCheckerApp.Services;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text;

namespace StartCheckerApp.Views
{
    public partial class FullListPage : BaseRunnersPage
    {
        private bool _isRunning = true;

        // Implementace TimeLabel pro tuto stránku
        protected override Label TimeLabel => TimeLabelControl;

        // Implementace CollectionView pro tuto stránku
        protected override CollectionView RunnersCollectionView => RunnersList;

        public FullListPage(RaceDataService raceDataService, HttpClient httpClient, RunnerDatabaseService runnerDatabase)
            : base(raceDataService, httpClient, runnerDatabase)
        {
            InitializeComponent();
            LoadAllRunnersAsync().ConfigureAwait(false);
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
        }

        private async void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            string query = e.NewTextValue?.ToLower().Trim();

            if (string.IsNullOrEmpty(query))
            {
                await LoadAllRunnersAsync();
                return;
            }

            var runners = await _runnerDatabase.GetRunnersAsync();

            var filtered = runners
                .Where(r =>
                    (r.FirstName?.ToLower().Contains(query) ?? false) ||
                    (r.Surname?.ToLower().Contains(query) ?? false) ||
                    (r.FullName?.ToLower().Contains(query) ?? false) ||
                    (r.Category?.ToLower().Contains(query) ?? false) ||
                    (r.RegistrationNumber?.ToLower().Contains(query) ?? false) ||
                    r.SINumber.ToString().Contains(query) ||
                    r.StartTime.ToString("HH:mm:ss").Contains(query))
                .ToList();

            var grouped = filtered
                .OrderBy(r => r.StartTime)
                .GroupBy(r => r.StartMinute)
                .Select(g => new RunnerGroup(g.Key, g))
                .ToList();

            RunnersList.ItemsSource = new ObservableCollection<RunnerGroup>(grouped);
        }

        private void OnClearSearchClicked(object sender, EventArgs e)
        {
            SearchEntry.Text = string.Empty;
            LoadAllRunnersAsync().ConfigureAwait(false);
        }
    }
}
