using StartCheckerApp.Models;
using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Controls;
using System.Collections.Generic;

namespace StartCheckerApp.Views
{
    public partial class FullListPage : ContentPage
    {
        private readonly RaceDataService _raceDataService;

        public FullListPage(RaceDataService raceDataService)
        {
            InitializeComponent();
            _raceDataService = raceDataService;

            // Naètení závodníkù ze služby
            RunnersList.ItemsSource = _raceDataService.Runners;
        }

        private async void OnRunnerSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.Count == 0)
                return;

            var selectedRunner = (Runner)e.CurrentSelection.FirstOrDefault();
            if (selectedRunner != null)
            {
                var popup = new RunnerDetailPopup(selectedRunner);
                var updatedRunner = await this.ShowPopupAsync(popup) as Runner;

                if (updatedRunner != null)
                {
                    // Aktualizace dat v RaceDataService
                    _raceDataService.UpdateRunner(updatedRunner);

                    // Aktualizace UI
                    RunnersList.ItemsSource = null;
                    RunnersList.ItemsSource = _raceDataService.Runners;
                }
            }

            ((CollectionView)sender).SelectedItem = null; // Zruší výbìr po kliknutí
        }
    }
}
