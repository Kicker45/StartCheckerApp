using Microsoft.Maui.Controls;

namespace StartCheckerApp
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private void OnLoadStartListClicked(object sender, EventArgs e)
        {
            // Akce při kliknutí na tlačítko "Načíst startovní listinu"
            DisplayAlert("Načíst", "Funkce načtení startovní listiny zatím není implementována.", "OK");
        }
    }
}
