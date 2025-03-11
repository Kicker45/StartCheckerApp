using Microsoft.Extensions.Logging;
using StartCheckerApp.Views;
using StartCheckerApp.Services;
using CommunityToolkit.Maui;
using SQLite;



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

            // Inicializace lokální SQLite databáze
            var database = new SQLiteAsyncConnection(Constants.DatabasePath, Constants.Flags);
            builder.Services.AddSingleton(database);
            builder.Services.AddSingleton<RunnerDatabaseService>();

            // Přidání http clienta
            var clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;

            builder.Services.AddSingleton(sp =>
            {
                return new HttpClient
                {
                    BaseAddress = new Uri(Constants.RestUrl),
                    Timeout = TimeSpan.FromSeconds(5),
                    DefaultRequestHeaders = { Connection = { "keep-alive" } }
                };
            });

            // Přidání služeb pro správu startovky
            builder.Services.AddSingleton<RaceDataService>();
            builder.Services.AddSingleton<StatusToColorConverter>();

            // Přidání toolkit pro správné fungování popup oken
            builder.UseMauiApp<App>().UseMauiCommunityToolkit();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
