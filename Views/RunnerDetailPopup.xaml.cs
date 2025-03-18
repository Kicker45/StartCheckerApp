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

            // Napln�n� vstupn�ch pol� daty z�vodn�ka
            RegistrationNumberEntry.Text = _runner.RegistrationNumber;
            FirstNameEntry.Text = _runner.FirstName;
            SurnameEntry.Text = _runner.Surname;
            CategoryEntry.Text = _runner.Category;
            SINumberEntry.Text = _runner.SINumber.ToString();

            // Nastaven� DatePicker a TimePicker
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

            // Spojen� vybran�ho data a �asu do jednoho DateTime objektu
            DateTime selectedDateTime = new DateTime(
                StartDatePicker.Date.Year,
                StartDatePicker.Date.Month,
                StartDatePicker.Date.Day,
                StartTimePicker.Time.Hours,
                StartTimePicker.Time.Minutes,
                StartTimePicker.Time.Seconds
            );

            // Konverze na UTC a ulo�en� do modelu
            _runner.StartTime = DateTime.SpecifyKind(selectedDateTime, DateTimeKind.Utc);

            if (_runner.ID == 0)
            {
                // Nov� z�vodn�k -> Ulo�it do lok�ln� DB
                await _runnerDatabase.AddRunnerAsync(_runner);
            }
            else
            {
                // Existuj�c� z�vodn�k -> Ulo�it aktualizaci do lok�ln� DB
                await _runnerDatabase.UpdateRunnerAsync(_runner);
            }

            Close(200); // Vr�t�me �sp�n� k�d odpov�di
        }

        private void OnCloseClicked(object sender, EventArgs e)
        {
            Close(null); // Zav�e popup bez zm�n
        }

        private async void OnDNSClicked(object sender, EventArgs e)
        {
            // Nastaven� p��znaku DNS
            _runner.DNS = true;

            // Pokud m�l z�vodn�k zaznamenan� start, mus�me ho vymazat
            _runner.StartPassage = null;
            _runner.Started = false;
            _runner.Late = false;
            _runner.StartFlag = false;

            // Aktualizace �asu posledn� zm�ny
            _runner.LastUpdatedAt = DateTime.UtcNow;

            // Ulo�en� zm�ny do lok�ln� datab�ze
            await _runnerDatabase.UpdateRunnerAsync(_runner);

            // Zav�en� popupu po ulo�en�
            Close(200);
        }
    }
}
