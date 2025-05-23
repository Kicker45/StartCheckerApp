﻿//------------------------------------------------------------------------------
// Název souboru: UsbCommunicationService.cs
// Autor: Jan Nechanický
// Popis: Tento soubor obsahuje obsluhu připojeného USB zařízení - nepovedlo se zprovoznit.
// Datum vytvoření: 1.4.2025
//------------------------------------------------------------------------------
namespace StartCheckerApp.Services
{
    public class UsbCommunicationService
    {
#if ANDROID
        private readonly UsbService _usbService;
#endif

        public event EventHandler<string> DataReceived;

        public UsbCommunicationService()
        {
#if ANDROID
            _usbService = new UsbService();
            _usbService.DataReceived += (data) => DataReceived?.Invoke(this, data);
#endif
        }

        public bool Start()
        {
#if ANDROID
            return _usbService.InitializeDevice();
#else
            return false;
#endif
        }

        public void BeginListening()
        {
#if ANDROID
            _usbService.StartListening();
#endif
        }

        public void Stop()
        {
#if ANDROID
            _usbService.StopListening();
#endif
        }
    }
}
