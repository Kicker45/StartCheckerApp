//------------------------------------------------------------------------------
// Název souboru: SettingsPage.xaml.cs
// Autor: Jan Nechanický
// Popis: Tento soubor obsahuje logiku pro stránku nastavení aplikace, vèetnì vytváøení závodu,
//       nahrávání startovní listiny, detekce USB zaøízení a uložení èasového posunu.
// Datum vytvoøení: 1.2.2025
//------------------------------------------------------------------------------

using StartCheckerApp.Services;
using Microsoft.Maui.Storage;

namespace StartCheckerApp.Views
{
    /// <summary>
    /// Stránka pro nastavení aplikace.
    /// </summary>
    public partial class SettingsPage : ContentPage
    {
        private readonly RaceDataService _raceDataService;
        private readonly IMessageService _messageService;
        private int? _createdRaceId;

        /// <summary>
        /// Konstruktor, kde se injektují potøebné služby.
        /// </summary>
        public SettingsPage(RaceDataService raceDataService, IMessageService messageService)
        {
            InitializeComponent();
            _raceDataService = raceDataService;
            _messageService = messageService;
        }

        /// <summary>
        /// Obsluha tlaèítka pro vytvoøení nového závodu na serveru.
        /// </summary>
        private async void OnCreateRaceClicked(object sender, EventArgs e)
        {
            try
            {
                var raceName = RaceNameEntry.Text?.Trim();
                if (string.IsNullOrWhiteSpace(raceName))
                {
                    await _messageService.ShowMessageAsync("Zadejte název závodu.");
                    return;
                }

                var raceDate = RaceDatePicker.Date;
                var raceTime = RaceTimePicker.Time;
                var raceStart = new DateTime(
                    raceDate.Year, raceDate.Month, raceDate.Day,
                    raceTime.Hours, raceTime.Minutes, raceTime.Seconds,
                    DateTimeKind.Local
                );

                int? raceId = await _raceDataService.AddRaceAsync(raceName, raceStart);
                if (raceId.HasValue)
                {
                    _createdRaceId = raceId.Value;
                    await _messageService.ShowMessageAsync($"Závod byl úspìšnì vytvoøen.\nID závodu: {_createdRaceId}");
                }
                else
                {
                    await _messageService.ShowMessageAsync("Nepodaøilo se vytvoøit závod. Zkontrolujte pøipojení k serveru a opakujte akci.");
                }
            }
            catch (Exception ex)
            {
                await _messageService.ShowMessageAsync($"Nastala neoèekávaná chyba: {ex.Message}");
            }
        }

        /// <summary>
        /// Obsluha tlaèítka pro nahrání startovní listiny ze souboru.
        /// </summary>
        private async void OnUploadStartlistClicked(object sender, EventArgs e)
        {
            if (!int.TryParse(ExistingRaceIdEntry.Text, out int raceId))
            {
                await _messageService.ShowMessageAsync("Zadejte platné ID závodu.");
                return;
            }

            var file = await FilePicker.PickAsync(new PickOptions { PickerTitle = "Vyberte IOF XML 3.0 soubor" });
            if (file == null)
            {
                await _messageService.ShowMessageAsync("Nebyl vybrán žádný soubor.");
                return;
            }

            if (!file.FileName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
            {
                await _messageService.ShowMessageAsync("Vybraný soubor není ve formátu .xml.");
                return;
            }

            bool success = await _raceDataService.UploadStartlistAsync(raceId, file);
            if (success)
                await _messageService.ShowMessageAsync("Startovní listina byla úspìšnì nahrána.");
            else
                await _messageService.ShowMessageAsync("Chyba pøi nahrávání listiny.");
        }

        /// <summary>
        /// Obsluha tlaèítka pro detekci USB èteèky SportIdent (Android only).
        /// </summary>
        private void OnDetectUsbClicked(object sender, EventArgs e)
        {
#if ANDROID
            UsbDataLabel.Text = "Naèítání zaøízení...";
            // TODO: Volání USB èteèky pøes vlastní službu _usbService.StartReading()
#else
            UsbDataLabel.Text = "USB èteèka není podporována na této platformì.";
#endif
        }

        /// <summary>
        /// Obsluha tlaèítka pro uložení èasového posunu v nastavení aplikace.
        /// </summary>
        private async void OnSaveTimeOffsetClickedAsync(object sender, EventArgs e)
        {
            if (int.TryParse(TimeOffsetEntry.Text, out int timeOffset))
            {
                Preferences.Set("TimeOffset", timeOffset);
                await _messageService.ShowMessageAsync($"Nastavený èasový posun {timeOffset}");
            }
            else
            {
                await _messageService.ShowMessageAsync("Error: Nastavte správný èasový posun");
            }
        }
    }
}
