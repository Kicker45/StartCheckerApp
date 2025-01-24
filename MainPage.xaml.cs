using Microsoft.Maui.Controls;

namespace StartCheckerApp
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }
        private async void OnNavigateToGetStartlist(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new GetStartlistPage());
        }

        private async void OnNavigateToFullList(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new FullListPage());
        }

        private void OnLoadStartListClicked(object sender, EventArgs e)
        {
            // Akce při kliknutí na tlačítko "Načíst startovní listinu"
            DisplayAlert("Načíst", "Funkce načtení startovní listiny zatím není implementována.", "OK");
        }
    }
}
