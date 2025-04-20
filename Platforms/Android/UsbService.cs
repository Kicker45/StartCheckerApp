using Android.App;
using Android.Content;
using Android.Hardware.Usb;
using Android.Util;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using StartCheckerApp.Services;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Alerts;

namespace StartCheckerApp
{
    public class UsbService
    {
        private const int VendorId = 0x10C4;
        private const int ProductId = 0x800A;
        private const string ActionUsbPermission = "com.startchecker.USB_PERMISSION";

        private UsbManager? _usbManager;
        private UsbDeviceConnection? _connection;
        private UsbEndpoint? _endpointIn;
        private CancellationTokenSource? _cancellationTokenSource;

        public event Action<string>? DataReceived;

        public UsbService()
        {
            _usbManager = (UsbManager)Android.App.Application.Context.GetSystemService(Context.UsbService);
        }

        public bool InitializeDevice()
        {
            foreach (var device in _usbManager.DeviceList.Values)
            {
                if (device.VendorId == VendorId && device.ProductId == ProductId)
                {
                    if (!_usbManager.HasPermission(device))
                    {
                        var permissionIntent = PendingIntent.GetBroadcast(Android.App.Application.Context, 0,
                            new Intent(ActionUsbPermission), PendingIntentFlags.Immutable);

                        _usbManager.RequestPermission(device, permissionIntent);
                        return false;
                    }

                    var usbInterface = device.GetInterface(0);
                    _endpointIn = usbInterface.GetEndpoint(0); // Předpokládáme Bulk IN endpoint

                    _connection = _usbManager.OpenDevice(device);
                    if (_connection == null || !_connection.ClaimInterface(usbInterface, true))
                        return false;

                    return true;
                }
            }
            return false;
        }

        public void StartListening()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            Task.Run(() => Listen(_cancellationTokenSource.Token));
        }

        public void StopListening()
        {
            _cancellationTokenSource?.Cancel();
            _connection?.Close();
        }

        private void Listen(CancellationToken token)
        {
            var buffer = new byte[64];
            var frameBuffer = new List<byte>();

            while (!token.IsCancellationRequested)
            {
                int bytesRead = _connection!.BulkTransfer(_endpointIn, buffer, buffer.Length, 1000);

                if (bytesRead > 0)
                {
                    for (int i = 0; i < bytesRead; i++)
                    {
                        byte b = buffer[i];

                        if (b == 0x02) // STX
                            frameBuffer.Clear();

                        frameBuffer.Add(b);

                        if (b == 0x03) // ETX
                        {
                            byte[] frame = frameBuffer.ToArray();
                            frameBuffer.Clear();

                            // zkusíme dekódovat SI čip
                            if (SportIdentFrameParser.TryParseSiFrame(frame, frame.Length, out int siid))
                            {
                                Microsoft.Maui.Controls.Application.Current?.Dispatcher.Dispatch(async () =>
                                {
                                    await Toast.Make($"SI čip: {siid}", ToastDuration.Short).Show();
                                });

                            }
                            else
                            {
                                // fallback: výpis raw dat
                                string hex = BitConverter.ToString(frame);
                                Microsoft.Maui.Controls.Application.Current?.Dispatcher.Dispatch(async () =>
                                {
                                    await Toast.Make($"[RAW] {hex}", ToastDuration.Short).Show();
                                });

                            }
                        }
                    }
                }
            }
        }
    }
}
