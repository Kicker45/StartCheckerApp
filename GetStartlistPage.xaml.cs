namespace StartCheckerApp; 
public partial class GetStartlistPage : ContentPage
{
    public GetStartlistPage()
    {
        InitializeComponent();
    }

    private async void OnLoadClicked(object sender, EventArgs e)
    {
        var code = CodeEntry.Text;
        if (!string.IsNullOrEmpty(code))
        {
            await DisplayAlert("Naètení", $"Startovní listina pro závod {code} byla naètena.", "OK");
            // Pozdìji pøidáme logiku pro volání API.
        }
        else
        {
            await DisplayAlert("Chyba", "Prosím zadejte kód závodu.", "OK");
        }
    }
}
