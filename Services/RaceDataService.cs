//------------------------------------------------------------------------------
// Název souboru: RaceDataService.cs
// Autor: Jan Nechanický
// Popis: Tento soubor obsahuje logiku pro správu dat o závodech, synchronizaci se serverem a práci s databází závodníků.
// Datum vytvoření: 1.4.2025
//------------------------------------------------------------------------------

using StartCheckerApp.Models;
using StartCheckerApp.Services;
using System.Text;
using System.Text.Json;

namespace StartCheckerApp
{
    // Služba pro správu dat závodu a závodníků
    public class RaceDataService
    {
        private readonly RunnerDatabaseService _runnerDatabase;   // Služba pro práci s SQLite databází závodníků
        private readonly HttpClient _httpClient;                 // HTTP klient pro komunikaci se serverem
        public int RaceId { get; private set; }                  // ID aktuálního závodu
        public List<Runner> Runners { get; private set; } = new List<Runner>(); // Seznam závodníků
        public DateTime LastSyncTime { get; internal set; }      // Čas poslední synchronizace se serverem

        // Konstruktor třídy pro inicializaci služeb
        public RaceDataService(HttpClient httpClient, RunnerDatabaseService runnerDatabase)
        {
            _httpClient = httpClient;
            _runnerDatabase = runnerDatabase;
        }
        /// <summary>
        /// Asynchronní metoda pro přidání nového závodu na server
        /// </summary>
        public async Task<int?> AddRaceAsync(string raceName, DateTime raceStart)
        {
            try
            {
                string url = $"add-race?RaceName={Uri.EscapeDataString(raceName)}&RaceStart={raceStart.ToUniversalTime():O}";
                HttpResponseMessage response = await _httpClient.PostAsync(url, null);

                if (response.IsSuccessStatusCode)
                {
                    string responseJson = await response.Content.ReadAsStringAsync();
                    using var document = JsonDocument.Parse(responseJson);
                    var root = document.RootElement;

                    if (root.TryGetProperty("raceId", out var raceIdElement))
                    {
                        return raceIdElement.GetInt32();  // Vrátí ID nového závodu
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AddRaceAsync exception: {ex.Message}");  // Logování chyby
            }

            return null;  // Pokud došlo k chybě, vrací null
        }
        /// <summary>
        /// Asynchronní metoda pro nahrání startovní listiny na server
        /// </summary>
        public async Task<bool> UploadStartlistAsync(int raceId, FileResult file)
        {
            try
            {
                using var content = new MultipartFormDataContent();
                using var stream = await file.OpenReadAsync();

                var fileContent = new StreamContent(stream);
                fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/xml");

                content.Add(fileContent, "file", file.FileName);
                content.Add(new StringContent(raceId.ToString()), "raceID");

                var response = await _httpClient.PostAsync("upload-startlist", content);

                return response.IsSuccessStatusCode;  // Vrací true, pokud server odpověděl úspěšně
            }
            catch (Exception ex)
            {
                Console.WriteLine($"UploadStartlistAsync exception: {ex.Message}");  // Logování chyby
                return false;  // Pokud došlo k chybě, vrací false
            }
        }

        /// <summary>
        /// Nastaví závod a jeho startovní listinu do služby
        /// </summary>
        public void SetRace(int raceId, List<Runner> runners)
        {
            RaceId = raceId;
            Runners = runners;
        }

        /// <summary>
        /// Nastaví seznam závodníků (pokud není použitý SetRace)
        /// </summary>
        public void SetRunners(List<Runner> runners)
        {
            Runners = runners;
        }

        /// <summary>
        /// Aktualizuje závodníka v seznamu
        /// </summary>
        public void UpdateRunner(Runner updatedRunner)
        {
            int index = Runners.FindIndex(r => r.ID == updatedRunner.ID);
            if (index >= 0)
            {
                Runners[index] = updatedRunner;  // Aktualizuje závodníka na základě ID
            }
        }

        /// <summary>
        /// Asynchronní metoda pro přidání závodníka na server
        /// </summary>
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
                        runner.ID = Convert.ToInt32(serverResponse["runnerId"]);  // Nastaví nově vytvořené ID ze serveru
                        return 201;  // HTTP Created
                    }
                }

                return (int)response.StatusCode;  // Pokud odpověď není úspěšná, vrátí HTTP status code
            }
            catch
            {
                return 500;  // Interní chyba serveru
            }
        }

        /// <summary>
        /// Asynchronní metoda pro aktualizaci závodníka na serveru
        /// </summary>
        public async Task<int> UpdateRunnerOnServer(Runner runner)
        {
            try
            {
                var json = JsonSerializer.Serialize(runner);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await _httpClient.PatchAsync("update-runner", content);
                return (int)response.StatusCode;  // Vrátí status kód odpovědi
            }
            catch
            {
                return 500;  // Interní chyba serveru
            }
        }

        /// <summary>
        /// Ukládá startovní listinu do SQLite databáze
        /// </summary>
        public async Task SaveStartListToDatabase(int raceId, List<Runner> runners)
        {
            await _runnerDatabase.InitializeAsync();  // Zajistí vytvoření tabulky, pokud neexistuje

            foreach (var runner in runners)
            {
                runner.LastUpdatedAt = DateTime.UtcNow;  // Nastavíme timestamp změny
                var existingRunner = await _runnerDatabase.GetRunnerByIdAsync(runner.ID);

                if (existingRunner != null)
                {
                    await _runnerDatabase.UpdateRunnerAsync(runner);  // Aktualizujeme existujícího závodníka
                }
                else
                {
                    await _runnerDatabase.AddRunnerAsync(runner);  // Přidáme nového závodníka
                }
            }

            await _runnerDatabase.GetRunnersOrderedByStartMinuteAsync();
            await _runnerDatabase.CreateIndexOnStartMinuteAsync();

            LastSyncTime = DateTime.UtcNow;  // Aktualizace času poslední synchronizace
        }

        /// <summary>
        /// Asynchronní metoda pro synchronizaci závodníků se serverem
        /// </summary>
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

        /// <summary>
        /// Asynchronní metoda pro získání změn ze serveru
        /// </summary>
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

        /// <summary>
        /// Kontrola dostupnosti serveru
        /// </summary>
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
