using StartCheckerApp.Services;

namespace StartCheckerApp.Platforms.Windows
{
    public class MessageService : IMessageService
    {
        public async Task ShowMessageAsync(string message)
        {
            await Application.Current!.MainPage!.DisplayAlert("Zpráva", message, "OK");
        }
    }
}
