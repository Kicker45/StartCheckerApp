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
            await DisplayAlert("Na�ten�", $"Startovn� listina pro z�vod {code} byla na�tena.", "OK");
            // Pozd�ji p�id�me logiku pro vol�n� API.
        }
        else
        {
            await DisplayAlert("Chyba", "Pros�m zadejte k�d z�vodu.", "OK");
        }
    }
}
