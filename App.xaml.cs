//------------------------------------------------------------------------------
// Název souboru: App.xaml.cs
// Autor: Jan Nechanický
// Popis: Tento soubor obsahuje inicializaci hlavních komponent.
// Datum vytvoření: 1.4.2025
//------------------------------------------------------------------------------

using SQLite;
using StartCheckerApp.Models;
using StartCheckerApp.Views;

namespace StartCheckerApp
{
    public partial class App : Application
    {
        private readonly SQLiteAsyncConnection _db;

        public App(MainPage mainPage, SQLiteAsyncConnection db)
        {
            InitializeComponent();
            App.Current.UserAppTheme = AppTheme.Light; // ⬅️ Nutné pro globální světlé zobrazení pro lepší čitelnost
            MainPage = new NavigationPage(mainPage);
            _db = db;
        }

        protected override void OnStart()
        {
            base.OnStart();
            // Smazání a znovuvytvoření tabulky
            Task.Run(async () =>
            {
                await _db.DropTableAsync<Runner>();
                await _db.CreateTableAsync<Runner>();
            }).Wait();
        }
    }

}
