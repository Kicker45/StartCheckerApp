using StartCheckerApp.Services;

namespace StartCheckerApp.Views;

public partial class SettingsPage : ContentPage
{
    private readonly RaceDataService _raceDataService;
    private readonly IMessageService _messageService;
    private int? _createdRaceId;
    int raceId;

    public SettingsPage(RaceDataService raceDataService, IMessageService messageService)
    {
        InitializeComponent();
        _raceDataService = raceDataService;
        _messageService = messageService;

    }

    private async void OnCreateRaceClicked(object sender, EventArgs e)
    {
#if WINDOWS
        var raceName = RaceNameEntry.Text;
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

        var raceId = await _raceDataService.AddRaceAsync(raceName, raceStart);
        if (raceId.HasValue)
        {
            _createdRaceId = raceId.Value;
            await _messageService.ShowMessageAsync($"Závod vytvoøen. ID: {_createdRaceId}");
        }
        else
        {
            await _messageService.ShowMessageAsync("Závod se nepodaøilo vytvoøit.");
        }
#endif
    }


    private async void OnUploadStartlistClicked(object sender, EventArgs e)
    {
        if (!int.TryParse(ExistingRaceIdEntry.Text, out int raceId))
        {
            await _messageService.ShowMessageAsync("Zadejte platné ID závodu.");
            return;
        }

        var file = await FilePicker.PickAsync(new PickOptions
        {
            PickerTitle = "Vyberte IOF XML 3.0 soubor"
        });

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



    private void OnDetectUsbClicked(object sender, EventArgs e)
    {
#if ANDROID
        // Pokud používáš vlastní Android USB službu:
        UsbDataLabel.Text = "Naèítání zaøízení...";
        // volání USB èteèky pøes _usbService.StartReading() apod.
#else
        UsbDataLabel.Text = "USB èteèka není podporována na této platformì.";
#endif
    }
}
