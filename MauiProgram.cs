using Microsoft.Extensions.Logging;
using StartCheckerApp.Views;
using CommunityToolkit.Maui;



namespace StartCheckerApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            // Nastavení NavigationPage
            builder.Services.AddSingleton<MainPage>();
            builder.Services.AddTransient<GetStartlistPage>();
            builder.Services.AddTransient<RunnerDetailPopup>();
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
                    BaseAddress = new Uri(Constants.RestUrl),
                    Timeout = TimeSpan.FromSeconds(10),
                    DefaultRequestHeaders = { Connection = { "keep-alive" } }
                };
            });
            // Přidání služby pro správu startovky
            builder.Services.AddSingleton<RaceDataService>();


            // Přidání toolkit pro správné fungování popup oken
            builder.UseMauiApp<App>().UseMauiCommunityToolkit();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
