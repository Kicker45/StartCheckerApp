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
            await Toast.Make("�te�ka p�ipravena ke �ten�.", ToastDuration.Short).Show();

        }
        else
        {
            await Toast.Make("Za��zen� nebylo nalezeno nebo chyb� opr�vn�n�.", ToastDuration.Short).Show();
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
