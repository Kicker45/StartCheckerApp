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

        public GetStartlistPage(HttpClient httpClient)
        {
            InitializeComponent();
            _httpClient = httpClient;
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
                string url = $"get-startlist?raceId={raceId}";
                Console.WriteLine($"Requesting: {_httpClient.BaseAddress}{url}");

                HttpResponseMessage response = await _httpClient.GetAsync(url);

                Console.WriteLine($"Response status: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    string responseData = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Server response: {responseData}");
                
                var raceData = JsonSerializer.Deserialize<RaceData>(responseData, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    await DisplayAlert("⁄spÏch", $"Z·vod: {raceData.RaceName}, PoËet z·vodnÌk˘: {raceData.StartList.Count}", "OK");
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
