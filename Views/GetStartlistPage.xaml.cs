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
        private readonly IMessageService _messageService;

        public GetStartlistPage(HttpClient httpClient, RaceDataService raceDataService, RunnerDatabaseService runnerDatabaseService, IMessageService messageService)
        {
            InitializeComponent();
            _httpClient = httpClient;
            _raceDataService = raceDataService;
            _runnerDatabaseService = runnerDatabaseService;
            _messageService = messageService;
        }

        private async void OnLoadStartListClicked(object sender, EventArgs e)
        {
            string raceId = RaceCodeEntry.Text;

            if (string.IsNullOrWhiteSpace(raceId))
            {
                await _messageService.ShowMessageAsync("Zadejte ID závodu!");
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
                        await _runnerDatabaseService.DeleteAllRunnersAsync();
                        _raceDataService.SetRace(raceData.RaceId, raceData.StartList);
                        await _raceDataService.SaveStartListToDatabase(raceData.RaceId, raceData.StartList);

                        await _messageService.ShowMessageAsync($"Závod: {raceData.RaceName}, Poèet závodníkù: {raceData.StartList.Count}");

                        LoadingIndicator.IsRunning = false;
                        LoadingIndicator.IsVisible = false;

                        await Navigation.PopAsync();
                        await Navigation.PushAsync(new FullListPage(_raceDataService, _httpClient, _runnerDatabaseService));
                    }
                    else
                    {
                        await _messageService.ShowMessageAsync($"Závod ({raceData.RaceName}) nemá žádné závodníky k zobrazení");
                    }
                }
                else
                {
                    await _messageService.ShowMessageAsync($"Chyba pøi komunikaci se serverem. Status: {response.StatusCode}");
                }
            }
            catch (HttpRequestException ex)
            {
                await _messageService.ShowMessageAsync($"Chyba HTTP: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Obecná chyba: {ex.Message}");
                await _messageService.ShowMessageAsync($"Chyba: {ex.Message}");
            }
            finally
            {
                LoadingIndicator.IsRunning = false;
                LoadingIndicator.IsVisible = false;
            }
        }
    }
}
