using SQLite;
using StartCheckerApp.Views;
using StartCheckerApp.Models;

namespace StartCheckerApp
{
    public partial class App : Application
    {
        private readonly SQLiteAsyncConnection _db;

        public App(MainPage mainPage, SQLiteAsyncConnection db)
        {
            InitializeComponent();
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
