using CommunityToolkit.Maui.Views;
using StartCheckerApp.Models;
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

        public RunnerDetailPopup(Runner runner, HttpClient httpClient, RaceDataService raceDataService)
        {
            InitializeComponent();
            _runner = runner;
            _httpClient = httpClient;
            _raceDataService = raceDataService;

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

            int responseCode;
            if (_runner.ID == 0)
            {
                // Nový závodník -> POST
                responseCode = await _raceDataService.AddRunnerToServer(_runner);
            }
            else
            {
                // Existující závodník -> PATCH
                responseCode = await _raceDataService.UpdateRunnerOnServer(_runner);
            }

            Close(responseCode); // Vrátíme kód odpovìdi API
        }


        private void OnCloseClicked(object sender, EventArgs e)
        {
            Close(null); // Zavøe popup bez zmìn
        }
    }
}
