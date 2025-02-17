using Microsoft.Extensions.Logging;
using StartCheckerApp.Views;

namespace StartCheckerApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            // Nastavení NavigationPage
            builder.Services.AddSingleton<MainPage>();
            builder.Services.AddTransient<GetStartlistPage>();
            builder.Services.AddTransient<RunnerDetailPage>();
            builder.Services.AddTransient<FullListPage>();
            builder.Services.AddTransient<SettingsPage>();
            builder.Services.AddTransient<CurrentMinutePage>();

            // Přidání http clienta
            var clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;

            builder.Services.AddSingleton(sp =>
            {
                return new HttpClient
                {
                    BaseAddress = new Uri("http://10.0.2.2:32769/api/StartList/"),
                    Timeout = TimeSpan.FromSeconds(10),
                    DefaultRequestHeaders = { Connection = { "keep-alive" } }
                };
            });



#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
