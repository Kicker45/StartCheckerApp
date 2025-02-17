using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using StartCheckerApp.Models;
using StartCheckerApp;

namespace StartCheckerApp.Views
{
    public partial class GetStartlistPage : ContentPage
    {
        private readonly HttpClient _httpClient;
        private readonly RaceDataService _raceDataService;

        public GetStartlistPage(HttpClient httpClient, RaceDataService raceDataService)
        {
            InitializeComponent();
            _httpClient = httpClient;
            _raceDataService = raceDataService;
        }

        private async void OnLoadStartListClicked(object sender, EventArgs e)
        {
            string raceId = RaceCodeEntry.Text;

            if (string.IsNullOrWhiteSpace(raceId))
            {
                await DisplayAlert("Chyba", "Zadejte ID z·vodu!", "OK");
                return;
            }

            LoadingIndicator.IsVisible = true;
            LoadingIndicator.IsRunning = true;

            
            try
            {
                //Console.WriteLine($"Requesting: {_httpClient.BaseAddress}{url}");
                string url = $"get-startlist?raceId={raceId}";
                //Console.WriteLine($"RestURL: {Constants.RestUrl}");
                //Console.WriteLine($"Requesting: {url}");


                HttpResponseMessage response = await _httpClient.GetAsync(url);

                //Console.WriteLine($"Response status: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    string responseData = await response.Content.ReadAsStringAsync();
                    //Console.WriteLine($"Server response: {responseData}");
                
                    var raceData = JsonSerializer.Deserialize<RaceData>(responseData, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (raceData.StartList.Count > 0) //ve startovce jsou z·vodnÌci ke zobrazenÌ
                    {
                        _raceDataService.SetRunners(raceData.StartList); // UloûÌme seznam z·vodnÌk˘ p¯es sluûbu 
                        _raceDataService.SetRunners(raceData.StartList);
                        await Navigation.PushAsync(new FullListPage(_raceDataService));
                        await DisplayAlert("⁄spÏch", $"Z·vod: {raceData.RaceName}, PoËet z·vodnÌk˘: {raceData.StartList.Count}", "OK");
                    }
                    else { await DisplayAlert("Chyba", $"Z·vod ({raceData.RaceName}) nem· û·dnÈ z·vodnÌky k zobrazenÌ", "OK"); }
                }
                else
                {
                    await DisplayAlert("Chyba", $"Chyba p¯i komunikaci se serverem. Status: {response.StatusCode}", "OK");
                }
            }
            catch (HttpRequestException ex)
            {
                await DisplayAlert("SÌùov· chyba", $"Chyba HTTP: {ex.Message}", "OK");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Obecn· chyba: {ex.Message}");
                await DisplayAlert("Chyba p¯ipojenÌ", $"Obecn· chyba: {ex.Message}", "OK");
            }
            finally
            {
                LoadingIndicator.IsRunning = false;
                LoadingIndicator.IsVisible = false;
            }
        }

    }
}
