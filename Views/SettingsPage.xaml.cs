//------------------------------------------------------------------------------
// N�zev souboru: SettingsPage.xaml.cs
// Autor: Jan Nechanick�
// Popis: Tento soubor obsahuje logiku pro str�nku nastaven� aplikace, v�etn� vytv��en� z�vodu,
//       nahr�v�n� startovn� listiny, detekce USB za��zen� a ulo�en� �asov�ho posunu.
// Datum vytvo�en�: 1.2.2025
//------------------------------------------------------------------------------

using StartCheckerApp.Services;
using Microsoft.Maui.Storage;

namespace StartCheckerApp.Views
{
    /// <summary>
    /// Str�nka pro nastaven� aplikace.
    /// </summary>
    public partial class SettingsPage : ContentPage
    {
        private readonly RaceDataService _raceDataService;
        private readonly IMessageService _messageService;
        private int? _createdRaceId;

        /// <summary>
        /// Konstruktor, kde se injektuj� pot�ebn� slu�by.
        /// </summary>
        public SettingsPage(RaceDataService raceDataService, IMessageService messageService)
        {
            InitializeComponent();
            _raceDataService = raceDataService;
            _messageService = messageService;
        }

        /// <summary>
        /// Obsluha tla��tka pro vytvo�en� nov�ho z�vodu na serveru.
        /// </summary>
        private async void OnCreateRaceClicked(object sender, EventArgs e)
        {
            try
            {
                var raceName = RaceNameEntry.Text?.Trim();
                if (string.IsNullOrWhiteSpace(raceName))
                {
                    await _messageService.ShowMessageAsync("Zadejte n�zev z�vodu.");
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
                    await _messageService.ShowMessageAsync($"Z�vod byl �sp�n� vytvo�en.\nID z�vodu: {_createdRaceId}");
                }
                else
                {
                    await _messageService.ShowMessageAsync("Nepoda�ilo se vytvo�it z�vod. Zkontrolujte p�ipojen� k serveru a opakujte akci.");
                }
            }
            catch (Exception ex)
            {
                await _messageService.ShowMessageAsync($"Nastala neo�ek�van� chyba: {ex.Message}");
            }
        }

        /// <summary>
        /// Obsluha tla��tka pro nahr�n� startovn� listiny ze souboru.
        /// </summary>
        private async void OnUploadStartlistClicked(object sender, EventArgs e)
        {
            if (!int.TryParse(ExistingRaceIdEntry.Text, out int raceId))
            {
                await _messageService.ShowMessageAsync("Zadejte platn� ID z�vodu.");
                return;
            }

            var file = await FilePicker.PickAsync(new PickOptions { PickerTitle = "Vyberte IOF XML 3.0 soubor" });
            if (file == null)
            {
                await _messageService.ShowMessageAsync("Nebyl vybr�n ��dn� soubor.");
                return;
            }

            if (!file.FileName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
            {
                await _messageService.ShowMessageAsync("Vybran� soubor nen� ve form�tu .xml.");
                return;
            }

            bool success = await _raceDataService.UploadStartlistAsync(raceId, file);
            if (success)
                await _messageService.ShowMessageAsync("Startovn� listina byla �sp�n� nahr�na.");
            else
                await _messageService.ShowMessageAsync("Chyba p�i nahr�v�n� listiny.");
        }

        /// <summary>
        /// Obsluha tla��tka pro detekci USB �te�ky SportIdent (Android only).
        /// </summary>
        private void OnDetectUsbClicked(object sender, EventArgs e)
        {
#if ANDROID
            UsbDataLabel.Text = "Na��t�n� za��zen�...";
            // TODO: Vol�n� USB �te�ky p�es vlastn� slu�bu _usbService.StartReading()
#else
            UsbDataLabel.Text = "USB �te�ka nen� podporov�na na t�to platform�.";
#endif
        }

        /// <summary>
        /// Obsluha tla��tka pro ulo�en� �asov�ho posunu v nastaven� aplikace.
        /// </summary>
        private async void OnSaveTimeOffsetClickedAsync(object sender, EventArgs e)
        {
            if (int.TryParse(TimeOffsetEntry.Text, out int timeOffset))
            {
                Preferences.Set("TimeOffset", timeOffset);
                await _messageService.ShowMessageAsync($"Nastaven� �asov� posun {timeOffset}");
            }
            else
            {
                await _messageService.ShowMessageAsync("Error: Nastavte spr�vn� �asov� posun");
            }
        }
    }
}
