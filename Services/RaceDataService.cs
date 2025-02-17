using StartCheckerApp.Models;
using System.Text;
using System.Text.Json;

namespace StartCheckerApp
{
    public class RaceDataService
    {
        private readonly HttpClient _httpClient;
        public int RaceId { get; private set; } // ID aktuálního závodu
        public List<Runner> Runners { get; private set; } = new List<Runner>();

        public RaceDataService(HttpClient httpClient)
        {
            _httpClient = httpClient;
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

    }
}
