using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using StartCheckerApp.Services;

namespace StartCheckerApp.Platforms.Android;

public class MessageService : IMessageService
{
    public async Task ShowMessageAsync(string message)
    {
        await Toast.Make(message, ToastDuration.Long).Show();
    }
}
