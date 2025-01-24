using Microsoft.Extensions.Logging;

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

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
