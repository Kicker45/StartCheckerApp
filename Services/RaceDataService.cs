using StartCheckerApp.Models;
using StartCheckerApp.Services;
using System.Text;
using System.Text.Json;

namespace StartCheckerApp
{
    public class RaceDataService
    {
        private readonly RunnerDatabaseService _runnerDatabase;
        private readonly HttpClient _httpClient;
        public int RaceId { get; private set; } // ID aktuálního závodu
        public List<Runner> Runners { get; private set; } = new List<Runner>();
        public DateTime LastSyncTime { get; internal set; }

        public RaceDataService(HttpClient httpClient, RunnerDatabaseService runnerDatabase)
        {
            _httpClient = httpClient;
            _runnerDatabase = runnerDatabase;
        }
        public void SetRace(int raceId, List<Runner> runners)
        {
            RaceId = raceId;
            Runners = runners;
        }
        public void SetRunners(List<Runner> runners) //TODO OBSOLETE?
        {
            Runners = runners;
        }
        public void UpdateRunner(Runner updatedRunner)
        {
            int index = Runners.FindIndex(r => r.ID == updatedRunner.ID);
            if (index >= 0)
            {
                Runners[index] = updatedRunner;
            }
        }

        public void AddRunner(Runner runner)
        {
            Runners.Add(runner);
        }

        public async Task<int> AddRunnerToServer(Runner runner)
        {
            try
            {
                var json = JsonSerializer.Serialize(runner);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await _httpClient.PostAsync("add-runner", content);
                if (response.IsSuccessStatusCode)
                {
                    Runners.Add(runner); // Přidání do lokálního seznamu po úspěšném přidání na server
                }

                return (int)response.StatusCode;
            }
            catch
            {
                return 500; // Interní chyba serveru
            }
        }
        public async Task<int> UpdateRunnerOnServer(Runner runner)
        {
            try
            {
                var json = JsonSerializer.Serialize(runner);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await _httpClient.PatchAsync("update-runner", content);
                return (int)response.StatusCode;
            }
            catch
            {
                return 500; // Interní chyba serveru
            }
        }

        public async Task SaveStartListToDatabase(int raceId, List<Runner> runners)
        {
            await _runnerDatabase.InitializeAsync(); // Zajistí vytvoření tabulky, pokud neexistuje

            foreach (var runner in runners)
            {
                runner.LastModified = DateTime.UtcNow; // Nastavíme timestamp změny
                var existingRunner = await _runnerDatabase.GetRunnerByIdAsync(runner.ID);

                if (existingRunner != null)
                {
                    await _runnerDatabase.UpdateRunnerAsync(runner); // Aktualizujeme existujícího závodníka
                }
                else
                {
                    await _runnerDatabase.AddRunnerAsync(runner); // Přidáme nového závodníka
                }
            }

            LastSyncTime = DateTime.UtcNow; // Aktualizace času poslední synchronizace
        }


    }
}
