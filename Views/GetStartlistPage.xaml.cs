//------------------------------------------------------------------------------
// N�zev souboru: GetStartlistPage.xaml.cs
// Autor: Jan Nechanick�
// Popis: Tento soubor obsahuje logiku pro str�nku pro na�ten� startovn� listiny ze serveru.
// Datum vytvo�en�: 1.2.2025
//------------------------------------------------------------------------------

using StartCheckerApp.Models;
using StartCheckerApp.Services;
using System.Text.Json;

namespace StartCheckerApp.Views
{
    // Tato t��da zaji��uje na��t�n� startovn� listiny pro dan� z�vod
    public partial class GetStartlistPage : ContentPage
    {
        private readonly HttpClient _httpClient;              // HTTP klient pro komunikaci se serverem
        private readonly RaceDataService _raceDataService;    // Slu�ba pro pr�ci se z�vody
        private readonly RunnerDatabaseService _runnerDatabaseService;  // Slu�ba pro spr�vu z�vodn�k� v SQLite datab�zi
        private readonly IMessageService _messageService;     // Slu�ba pro zobrazov�n� zpr�v (nap�. notifikace)

        // Konstruktor t��dy, inicializuje v�echny pot�ebn� slu�by
        public GetStartlistPage(HttpClient httpClient, RaceDataService raceDataService, RunnerDatabaseService runnerDatabaseService, IMessageService messageService)
        {
            InitializeComponent();  // Inicializuje XAML komponenty
            _httpClient = httpClient;
            _raceDataService = raceDataService;
            _runnerDatabaseService = runnerDatabaseService;
            _messageService = messageService;
        }

        // Metoda, kter� se spust� p�i kliknut� na tla��tko "Na��st startovn� listinu"
        private async void OnLoadStartListClicked(object sender, EventArgs e)
        {
            string raceId = RaceCodeEntry.Text;  // Na�te ID z�vodu z textov�ho pole

            // Pokud nen� zadan� ID z�vodu, zobraz� se chybov� zpr�va
            if (string.IsNullOrWhiteSpace(raceId))
            {
                await _messageService.ShowMessageAsync("Zadejte ID z�vodu!");  // Zobrazen� zpr�vy u�ivateli
                return;  // Ukon��me metodu, pokud nen� zad�no ID
            }

            // Zobrazen� indik�toru na��t�n�
            LoadingIndicator.IsVisible = true;
            LoadingIndicator.IsRunning = true;

            try
            {
                // Vytvo�en� URL pro z�sk�n� startovn� listiny
                string url = $"get-startlist?raceId={raceId}";
                // Odesl�n� GET po�adavku na server
                HttpResponseMessage response = await _httpClient.GetAsync(url);

                // Kontrola, zda server odpov�d�l �sp�n�
                if (response.IsSuccessStatusCode)
                {
                    // Na�ten� odpov�di ze serveru
                    string responseData = await response.Content.ReadAsStringAsync();
                    // Deserializace JSON dat do objektu RaceData
                    var raceData = JsonSerializer.Deserialize<RaceData>(responseData, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    // Pokud startovn� listina obsahuje n�jak� z�vodn�ky
                    if (raceData.StartList.Count >= 0)
                    {
                        // Nejprve sma�eme v�echny existuj�c� z�vodn�ky v datab�zi
                        await _runnerDatabaseService.DeleteAllRunnersAsync();
                        // Ulo��me z�vodn�ky a jejich startovn� listinu do slu�by pro z�vod
                        _raceDataService.SetRace(raceData.RaceId, raceData.StartList);
                        // Ulo��me z�vodn�ky do lok�ln� datab�ze
                        await _raceDataService.SaveStartListToDatabase(raceData.RaceId, raceData.StartList);

                        // Zobrazen� zpr�vy o �sp�n�m na�ten�
                        await _messageService.ShowMessageAsync($"Z�vod: {raceData.RaceName}, Po�et z�vodn�k�: {raceData.StartList.Count}");

                        // Skryt� indik�toru na��t�n�
                        LoadingIndicator.IsRunning = false;
                        LoadingIndicator.IsVisible = false;

                        // Navigace zp�t na p�edchoz� str�nku
                        await Navigation.PopAsync();
                        // Navigace na FullListPage (zobraz� z�vodn�ky z SQLite)
                        await Navigation.PushAsync(new FullListPage(_raceDataService, _httpClient, _runnerDatabaseService));
                    }
                    else
                    {
                        // Pokud z�vod nem� ��dn� z�vodn�ky, zobraz� se chybov� zpr�va
                        await _messageService.ShowMessageAsync($"Z�vod ({raceData.RaceName}) nem� ��dn� z�vodn�ky k zobrazen�");
                    }
                }
                else
                {
                    // Pokud komunikace se serverem sel�e, zobraz� se chybov� zpr�va
                    await _messageService.ShowMessageAsync($"Chyba p�i komunikaci se serverem. Status: {response.StatusCode}");
                }
            }
            catch (HttpRequestException ex)
            {
                // Zobrazen� chybov� zpr�vy p�i chyb� v HTTP po�adavku
                await _messageService.ShowMessageAsync($"Chyba HTTP: {ex.Message}");
            }
            catch (Exception ex)
            {
                // Zobrazen� obecn� chybov� zpr�vy
                Console.WriteLine($"Obecn� chyba: {ex.Message}");
                await _messageService.ShowMessageAsync($"Chyba: {ex.Message}");
            }
            finally
            {
                // Skryt� indik�toru na��t�n� po dokon�en� operace
                LoadingIndicator.IsRunning = false;
                LoadingIndicator.IsVisible = false;
            }
        }
    }
}
