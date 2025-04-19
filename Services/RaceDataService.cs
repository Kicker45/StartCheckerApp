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

        public async Task<int> AddRunnerToServer(Runner runner)
        {
            try
            {
                var json = JsonSerializer.Serialize(runner);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await _httpClient.PostAsync("add-runner", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var serverResponse = JsonSerializer.Deserialize<Dictionary<string, object>>(responseContent);

                    if (serverResponse != null && serverResponse.ContainsKey("runnerId"))
                    {
                        runner.ID = Convert.ToInt32(serverResponse["runnerId"]); // Nastavíme nově vytvořené ID ze serveru
                        return 201; // HTTP Created
                    }
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
                runner.LastUpdatedAt = DateTime.UtcNow; // Nastavíme timestamp změny
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

        public async Task SyncRunnersWithServer()
        {
            bool isServerAvailable = await CheckServerAvailability();

            if (!isServerAvailable)
            {
                await Application.Current.MainPage.DisplayAlert("Synchronizace", "Nepodařilo se navázat spojení se serverem. Změny budou uchovány lokálně.", "OK");
                return;
            }

            // Odeslání lokálních změn
            var modifiedRunners = (await _runnerDatabase.GetRunnersAsync())
                .Where(r => r.LastUpdatedAt > LastSyncTime)
                .ToList();

            if (modifiedRunners.Count > 0)
            {
                foreach (var runner in modifiedRunners)
                {
                    if (runner.ID <= 0)
                    {
                        int statusCode = await AddRunnerToServer(runner);
                        if (statusCode == 201 || statusCode == 200)
                        {
                            runner.LastUpdatedAt = DateTime.UtcNow;
                            await _runnerDatabase.UpdateRunnerAsync(runner);
                        }
                    }
                    else
                    {
                        int statusCode = await UpdateRunnerOnServer(runner);
                        if (statusCode == 200)
                        {
                            runner.LastUpdatedAt = DateTime.UtcNow;
                            await _runnerDatabase.UpdateRunnerAsync(runner);
                        }
                    }
                }

                // Počkáme na zpracování na serveru
                await Task.Delay(2000);
            }

            // Stáhneme nové změny ze serveru
            DateTime currentSyncTime = DateTime.UtcNow;
            var serverUpdates = await FetchUpdatesFromServer(LastSyncTime);

            if (serverUpdates != null && serverUpdates.Count > 0)
            {
                await SaveStartListToDatabase(RaceId, serverUpdates);
            }

            // Aktualizujeme čas poslední synchronizace
            LastSyncTime = currentSyncTime;
            await Application.Current.MainPage.DisplayAlert("Synchronizace", "Všechny změny byly úspěšně synchronizovány.", "OK");
        }


        public async Task<List<Runner>> FetchUpdatesFromServer(DateTime lastSyncTime)
        {
            try
            {
                string url = $"get-updates?raceId={RaceId}&lastSyncTime={lastSyncTime:o}";
                HttpResponseMessage response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    string responseData = await response.Content.ReadAsStringAsync();
                    var responseJson = JsonSerializer.Deserialize<Dictionary<string, object>>(responseData, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (responseJson != null && responseJson.ContainsKey("runners"))
                    {
                        var runnersJson = JsonSerializer.Serialize(responseJson["runners"]);
                        var updatedRunners = JsonSerializer.Deserialize<List<Runner>>(runnersJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                        return updatedRunners ?? new List<Runner>();
                    }
                }

                await Application.Current.MainPage.DisplayAlert("Chyba", $"Nepodařilo se získat změny ze serveru. Status: {response.StatusCode}", "OK");
                return new List<Runner>();
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Chyba", $"Chyba při načítání změn ze serveru: {ex.Message}", "OK");
                return new List<Runner>();
            }
        }

        public async Task<bool> CheckServerAvailability()
        {
            try
            {
                string url = $"get-startlist?raceId={RaceId}";
                HttpResponseMessage response = await _httpClient.GetAsync(url);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

    }
}
