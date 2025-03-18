using System.Text.Json;
using StartCheckerApp.Models;
using StartCheckerApp.Services;

namespace StartCheckerApp.Views
{
    public partial class GetStartlistPage : ContentPage
    {
        private readonly HttpClient _httpClient;
        private readonly RaceDataService _raceDataService;
        private readonly RunnerDatabaseService _runnerDatabaseService;

        public GetStartlistPage(HttpClient httpClient, RaceDataService raceDataService, RunnerDatabaseService runnerDatabaseService)
        {
            InitializeComponent();
            _httpClient = httpClient;
            _raceDataService = raceDataService;
            _runnerDatabaseService = runnerDatabaseService;
        }

        private async void OnLoadStartListClicked(object sender, EventArgs e)
        {
            string raceId = RaceCodeEntry.Text;

            if (string.IsNullOrWhiteSpace(raceId))
            {
                await DisplayAlert("Chyba", "Zadejte ID z�vodu!", "OK");
                return;
            }

            LoadingIndicator.IsVisible = true;
            LoadingIndicator.IsRunning = true;

            try
            {
                string url = $"get-startlist?raceId={raceId}";
                HttpResponseMessage response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    string responseData = await response.Content.ReadAsStringAsync();
                    var raceData = JsonSerializer.Deserialize<RaceData>(responseData, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (raceData.StartList.Count > 0)
                    {
                        // Nastaven� RaceId v RaceDataService pro synchronizaci
                        _raceDataService.SetRace(raceData.RaceId, raceData.StartList);

                        // Ulo�it do SQLite m�sto p��m�ho pou�it�
                        await _raceDataService.SaveStartListToDatabase(raceData.RaceId, raceData.StartList);

                        LoadingIndicator.IsRunning = false;
                        LoadingIndicator.IsVisible = false;
                        await DisplayAlert("�sp�ch", $"Z�vod: {raceData.RaceName}, Po�et z�vodn�k�: {raceData.StartList.Count}", "OK");
                        await Navigation.PopAsync();
                        // Navigace na FullListPage (kter� u� pracuje se SQLite)
                        await Navigation.PushAsync(new FullListPage(_raceDataService, _httpClient, _runnerDatabaseService));
                    }
                    else
                    {
                        await DisplayAlert("Chyba", $"Z�vod ({raceData.RaceName}) nem� ��dn� z�vodn�ky k zobrazen�", "OK");
                    }
                }
                else
                {
                    await DisplayAlert("Chyba", $"Chyba p�i komunikaci se serverem. Status: {response.StatusCode}", "OK");
                }
            }
            catch (HttpRequestException ex)
            {
                await DisplayAlert("S�ov� chyba", $"Chyba HTTP: {ex.Message}", "OK");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Obecn� chyba: {ex.Message}");
                await DisplayAlert("Chyba p�ipojen�", $"Obecn� chyba: {ex.Message}", "OK");
            }
            finally
            {
                LoadingIndicator.IsRunning = false;
                LoadingIndicator.IsVisible = false;
            }
        }

    }
}
