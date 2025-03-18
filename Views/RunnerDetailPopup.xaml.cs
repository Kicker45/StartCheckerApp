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

            // Nastavení DatePicker a TimePicker
            StartDatePicker.Date = _runner.StartTime.ToLocalTime().Date;
            StartTimePicker.Time = _runner.StartTime.ToLocalTime().TimeOfDay;
        }

        private async void OnSaveClicked(object sender, EventArgs e)
        {
            _runner.RegistrationNumber = RegistrationNumberEntry.Text;
            _runner.FirstName = FirstNameEntry.Text;
            _runner.Surname = SurnameEntry.Text;
            _runner.Category = CategoryEntry.Text;
            _runner.SINumber = int.Parse(SINumberEntry.Text);

            // Spojení vybraného data a èasu do jednoho DateTime objektu
            DateTime selectedDateTime = new DateTime(
                StartDatePicker.Date.Year,
                StartDatePicker.Date.Month,
                StartDatePicker.Date.Day,
                StartTimePicker.Time.Hours,
                StartTimePicker.Time.Minutes,
                StartTimePicker.Time.Seconds
            );

            // Konverze na UTC a uložení do modelu
            _runner.StartTime = DateTime.SpecifyKind(selectedDateTime, DateTimeKind.Utc);

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

        private async void OnDNSClicked(object sender, EventArgs e)
        {
            // Nastavení pøíznaku DNS
            _runner.DNS = true;

            // Pokud mìl závodník zaznamenaný start, musíme ho vymazat
            _runner.StartPassage = null;
            _runner.Started = false;
            _runner.Late = false;
            _runner.StartFlag = false;

            // Aktualizace èasu poslední zmìny
            _runner.LastUpdatedAt = DateTime.UtcNow;

            // Uložení zmìny do lokální databáze
            await _runnerDatabase.UpdateRunnerAsync(_runner);

            // Zavøení popupu po uložení
            Close(200);
        }
    }
}
