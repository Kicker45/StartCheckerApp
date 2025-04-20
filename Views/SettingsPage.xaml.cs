using StartCheckerApp.Services;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;


namespace StartCheckerApp.Views;
public partial class SettingsPage : ContentPage
{
    private readonly UsbCommunicationService _usbService;

    public SettingsPage(UsbCommunicationService usbService)
    {
        InitializeComponent();
        _usbService = usbService;
        _usbService.DataReceived += OnDataReceived;
    }

    private async void OnDetectUsbClicked(object sender, EventArgs e)
    {
        if (_usbService.Start())
        {
            _usbService.BeginListening();
            await Toast.Make("Èteèka pøipravena ke ètení.", ToastDuration.Short).Show();

        }
        else
        {
            await Toast.Make("Zaøízení nebylo nalezeno nebo chybí oprávnìní.", ToastDuration.Short).Show();
        }
    }

    private async void OnDataReceived(object sender, string data)
    {
        await Toast.Make(data, ToastDuration.Short).Show();
    }



    protected override void OnDisappearing()
    {
        _usbService.Stop();
        base.OnDisappearing();
    }
}
