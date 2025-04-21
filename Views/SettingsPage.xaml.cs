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

        var raceId = await _raceDataService.AddRaceAsync(raceName, raceStart);
        if (raceId.HasValue)
        {
            _createdRaceId = raceId.Value;
            await _messageService.ShowMessageAsync($"Z�vod vytvo�en. ID: {_createdRaceId}");
        }
        else
        {
            await _messageService.ShowMessageAsync("Z�vod se nepoda�ilo vytvo�it.");
        }
#endif
    }


    private async void OnUploadStartlistClicked(object sender, EventArgs e)
    {
        if (!int.TryParse(ExistingRaceIdEntry.Text, out int raceId))
        {
            await _messageService.ShowMessageAsync("Zadejte platn� ID z�vodu.");
            return;
        }

        var file = await FilePicker.PickAsync(new PickOptions
        {
            PickerTitle = "Vyberte IOF XML 3.0 soubor"
        });

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



    private void OnDetectUsbClicked(object sender, EventArgs e)
    {
#if ANDROID
        // Pokud pou��v� vlastn� Android USB slu�bu:
        UsbDataLabel.Text = "Na��t�n� za��zen�...";
        // vol�n� USB �te�ky p�es _usbService.StartReading() apod.
#else
        UsbDataLabel.Text = "USB �te�ka nen� podporov�na na t�to platform�.";
#endif
    }
}
