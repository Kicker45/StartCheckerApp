using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using SQLite;
using StartCheckerApp.Services;
using StartCheckerApp.Views;

namespace StartCheckerApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();

            // 1) Registrace CommunityToolkit a fontů
            builder
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            // 2) Registrace stránek (NavigationPage a další)
            builder.Services.AddSingleton<MainPage>();
            builder.Services.AddTransient<GetStartlistPage>();
            builder.Services.AddTransient<RunnerDetailPage>();
            builder.Services.AddTransient<FullListPage>();
            builder.Services.AddTransient<SettingsPage>();
            builder.Services.AddTransient<CurrentMinutePage>();


            // 3) Inicializace lokální SQLite databáze
            //    Vytvoříme instanci SQLiteAsyncConnection a přidáme ji jako singleton
            var database = new SQLiteAsyncConnection(Constants.DatabasePath, Constants.Flags);
            builder.Services.AddSingleton(database);

            // 4) Další služby související s DB nebo logikou
            builder.Services.AddSingleton<RunnerDatabaseService>();

            // 5) Přidání HTTP klienta (s nastavením callbacku pro certifikáty)
            var clientHandler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };
            builder.Services.AddSingleton(sp =>
            {
                return new HttpClient(clientHandler)
                {
                    BaseAddress = new Uri(Constants.RestUrl),
                    Timeout = TimeSpan.FromSeconds(5),
                    DefaultRequestHeaders = { Connection = { "keep-alive" } }
                };
            });

            // 6) Přidání dalších služeb
#if ANDROID
            builder.Services.AddSingleton<IMessageService, StartCheckerApp.Platforms.Android.MessageService>();
            builder.Services.AddSingleton<UsbCommunicationService>();
#elif WINDOWS
            builder.Services.AddSingleton<IMessageService, StartCheckerApp.Platforms.Windows.MessageService>();
#endif


            builder.Services.AddSingleton<RaceDataService>();
            builder.Services.AddSingleton<StatusToColorConverter>();

            // 7) Registrace třídy App do Dependency Injection (DI)
            //    - injektujeme MainPage a SQLiteAsyncConnection do App
            builder.Services.AddSingleton<App>(sp =>
            {
                var mainPage = sp.GetRequiredService<MainPage>();
                var dbConn = sp.GetRequiredService<SQLiteAsyncConnection>();
                return new App(mainPage, dbConn);
            });

            // 8) Logging v debug modu
#if DEBUG
            builder.Logging.AddDebug();
#endif

            // 9) Vytvoříme skutečnou MAUI aplikaci
            //    - použitím delegáta, který si z DI vyžádá App
            return builder
                .UseMauiApp(sp => sp.GetRequiredService<App>()) // sp => service provider
                .Build();
        }
    }
}
