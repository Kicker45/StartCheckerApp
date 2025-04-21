using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
