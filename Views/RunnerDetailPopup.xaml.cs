using CommunityToolkit.Maui.Views;
using StartCheckerApp.Models;
using StartCheckerApp.Services;
using System.Globalization;
using System.Text;
using System.Text.Json;

namespace StartCheckerApp.Views
{
    public partial class RunnerDetailPopup : Popup
    {
        private Runner _runner;
        private readonly HttpClient _httpClient;
        private readonly RaceDataService _raceDataService;
        private readonly RunnerDatabaseService _runnerDatabase;

        public RunnerDetailPopup(Runner runner, HttpClient httpClient, RaceDataService raceDataService, RunnerDatabaseService runnerDatabase)
        {
            InitializeComponent();
            _runner = runner;
            _httpClient = httpClient;
            _raceDataService = raceDataService;
            _runnerDatabase = runnerDatabase;

            // Naplnìní vstupních polí daty závodníka
            RegistrationNumberEntry.Text = _runner.RegistrationNumber;
            FirstNameEntry.Text = _runner.FirstName;
            SurnameEntry.Text = _runner.Surname;
            CategoryEntry.Text = _runner.Category;
            SINumberEntry.Text = _runner.SINumber.ToString();

            // Zobrazení èasu v HH:mm pro uživatele
            StartTimeEntry.Text = _runner.StartTime.ToLocalTime().ToString("HH:mm");
        }

        private async void OnSaveClicked(object sender, EventArgs e)
        {
            _runner.RegistrationNumber = RegistrationNumberEntry.Text;
            _runner.FirstName = FirstNameEntry.Text;
            _runner.Surname = SurnameEntry.Text;
            _runner.Category = CategoryEntry.Text;
            _runner.SINumber = int.Parse(SINumberEntry.Text);

            // Pøevod z HH:mm na UTC DateTime
            if (DateTime.TryParseExact(StartTimeEntry.Text, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedTime))
            {
                _runner.StartTime = DateTime.SpecifyKind(
                    new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, parsedTime.Hour, parsedTime.Minute, 0),
                    DateTimeKind.Utc
                );
            }
            else
            {
                Close(400); // Neplatný formát èasu, vracíme chybu
                return;
            }

            if (_runner.ID == 0)
            {
                // Nový závodník -> Uložit do lokální DB
                await _runnerDatabase.AddRunnerAsync(_runner);
            }
            else
            {
                // Existující závodník -> Uložit aktualizaci do lokální DB
                await _runnerDatabase.UpdateRunnerAsync(_runner);
            }

            Close(200); // Vrátíme úspìšný kód odpovìdi
        }


        private void OnCloseClicked(object sender, EventArgs e)
        {
            Close(null); // Zavøe popup bez zmìn
        }
    }
}
