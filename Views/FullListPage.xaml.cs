using StartCheckerApp.Models;
using CommunityToolkit.Maui.Views;
using System.ComponentModel;

namespace StartCheckerApp.Views
{
    public partial class FullListPage : ContentPage, INotifyPropertyChanged
    {
        private readonly RaceDataService _raceDataService;
        private readonly HttpClient _httpClient;
        private bool _isRunning = true;

        public FullListPage(RaceDataService raceDataService, HttpClient httpClient)
        {
            InitializeComponent();
            _raceDataService = raceDataService;
            _httpClient = httpClient;

            // Na�ten� z�vodn�k� ze slu�by
            RunnersList.ItemsSource = _raceDataService.Runners;

            // Synchronizace �asu
            UpdateTimeLoop();
        }

        private async void UpdateTimeLoop()
        {
            while (_isRunning)
            {
                TimeLabel.Text = DateTime.Now.ToString("HH:mm:ss");
                await Task.Delay(500); // Aktualizace ka�dou sekundu
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _isRunning = false; // Zastav�me aktualizaci �asu, kdy� u�ivatel opust� str�nku
        }

        private async void OnRunnerSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.Count == 0)
                return;

            var selectedRunner = (Runner)e.CurrentSelection.FirstOrDefault();
            if (selectedRunner != null)
            {
                await HandleRunnerPopup(selectedRunner);
            }

            ((CollectionView)sender).SelectedItem = null;
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

        private async Task HandleRunnerPopup(Runner runner)
        {
            var popup = new RunnerDetailPopup(runner, _httpClient, _raceDataService);
            var responseCode = await this.ShowPopupAsync(popup) as int?;

            if (responseCode.HasValue)
            {
                ShowResponseAlert(responseCode.Value);
                if (responseCode == 200)
                {
                    UpdateRunnerList();
                }
            }
        }

        private void UpdateRunnerList()
        {
            RunnersList.ItemsSource = null;
            RunnersList.ItemsSource = _raceDataService.Runners;
        }

        private async void ShowResponseAlert(int responseCode)
        {
            string message = responseCode switch
            {
                200 => "�sp�ch: Z�vodn�k byl �sp�n� aktualizov�n/p�id�n.",
                400 => "Chyba: Neplatn� data. Zkontrolujte zadan� �daje.",
                404 => "Chyba: Z�vodn�k nebyl nalezen.",
                409 => "Chyba: Z�vodn�k s t�mto ID ji� existuje.",
                _ => "Chyba: Nepoda�ilo se dokon�it operaci."
            };

            await DisplayAlert(responseCode == 200 ? "�sp�ch" : "Chyba", message, "OK");
        }
    }
}
