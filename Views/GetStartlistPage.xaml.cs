//------------------------------------------------------------------------------
// Název souboru: GetStartlistPage.xaml.cs
// Autor: Jan Nechanickı
// Popis: Tento soubor obsahuje logiku pro stránku pro naètení startovní listiny ze serveru.
// Datum vytvoøení: 1.2.2025
//------------------------------------------------------------------------------

using StartCheckerApp.Models;
using StartCheckerApp.Services;
using System.Text.Json;

namespace StartCheckerApp.Views
{
    // Tato tøída zajišuje naèítání startovní listiny pro danı závod
    public partial class GetStartlistPage : ContentPage
    {
        private readonly HttpClient _httpClient;              // HTTP klient pro komunikaci se serverem
        private readonly RaceDataService _raceDataService;    // Sluba pro práci se závody
        private readonly RunnerDatabaseService _runnerDatabaseService;  // Sluba pro správu závodníkù v SQLite databázi
        private readonly IMessageService _messageService;     // Sluba pro zobrazování zpráv (napø. notifikace)

        // Konstruktor tøídy, inicializuje všechny potøebné sluby
        public GetStartlistPage(HttpClient httpClient, RaceDataService raceDataService, RunnerDatabaseService runnerDatabaseService, IMessageService messageService)
        {
            InitializeComponent();  // Inicializuje XAML komponenty
            _httpClient = httpClient;
            _raceDataService = raceDataService;
            _runnerDatabaseService = runnerDatabaseService;
            _messageService = messageService;
        }

        // Metoda, která se spustí pøi kliknutí na tlaèítko "Naèíst startovní listinu"
        private async void OnLoadStartListClicked(object sender, EventArgs e)
        {
            string raceId = RaceCodeEntry.Text;  // Naète ID závodu z textového pole

            // Pokud není zadané ID závodu, zobrazí se chybová zpráva
            if (string.IsNullOrWhiteSpace(raceId))
            {
                await _messageService.ShowMessageAsync("Zadejte ID závodu!");  // Zobrazení zprávy uivateli
                return;  // Ukonèíme metodu, pokud není zadáno ID
            }

            // Zobrazení indikátoru naèítání
            LoadingIndicator.IsVisible = true;
            LoadingIndicator.IsRunning = true;

            try
            {
                // Vytvoøení URL pro získání startovní listiny
                string url = $"get-startlist?raceId={raceId}";
                // Odeslání GET poadavku na server
                HttpResponseMessage response = await _httpClient.GetAsync(url);

                // Kontrola, zda server odpovìdìl úspìšnì
                if (response.IsSuccessStatusCode)
                {
                    // Naètení odpovìdi ze serveru
                    string responseData = await response.Content.ReadAsStringAsync();
                    // Deserializace JSON dat do objektu RaceData
                    var raceData = JsonSerializer.Deserialize<RaceData>(responseData, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    // Pokud startovní listina obsahuje nìjaké závodníky
                    if (raceData.StartList.Count >= 0)
                    {
                        // Nejprve smaeme všechny existující závodníky v databázi
                        await _runnerDatabaseService.DeleteAllRunnersAsync();
                        // Uloíme závodníky a jejich startovní listinu do sluby pro závod
                        _raceDataService.SetRace(raceData.RaceId, raceData.StartList);
                        // Uloíme závodníky do lokální databáze
                        await _raceDataService.SaveStartListToDatabase(raceData.RaceId, raceData.StartList);

                        // Zobrazení zprávy o úspìšném naètení
                        await _messageService.ShowMessageAsync($"Závod: {raceData.RaceName}, Poèet závodníkù: {raceData.StartList.Count}");

                        // Skrytí indikátoru naèítání
                        LoadingIndicator.IsRunning = false;
                        LoadingIndicator.IsVisible = false;

                        // Navigace zpìt na pøedchozí stránku
                        await Navigation.PopAsync();
                        // Navigace na FullListPage (zobrazí závodníky z SQLite)
                        await Navigation.PushAsync(new FullListPage(_raceDataService, _httpClient, _runnerDatabaseService));
                    }
                    else
                    {
                        // Pokud závod nemá ádné závodníky, zobrazí se chybová zpráva
                        await _messageService.ShowMessageAsync($"Závod ({raceData.RaceName}) nemá ádné závodníky k zobrazení");
                    }
                }
                else
                {
                    // Pokud komunikace se serverem sele, zobrazí se chybová zpráva
                    await _messageService.ShowMessageAsync($"Chyba pøi komunikaci se serverem. Status: {response.StatusCode}");
                }
            }
            catch (HttpRequestException ex)
            {
                // Zobrazení chybové zprávy pøi chybì v HTTP poadavku
                await _messageService.ShowMessageAsync($"Chyba HTTP: {ex.Message}");
            }
            catch (Exception ex)
            {
                // Zobrazení obecné chybové zprávy
                Console.WriteLine($"Obecná chyba: {ex.Message}");
                await _messageService.ShowMessageAsync($"Chyba: {ex.Message}");
            }
            finally
            {
                // Skrytí indikátoru naèítání po dokonèení operace
                LoadingIndicator.IsRunning = false;
                LoadingIndicator.IsVisible = false;
            }
        }
    }
}
