using CommunityToolkit.Mvvm.Messaging;
using StartCheckerApp.Messages;
using StartCheckerApp.Models;
using StartCheckerApp.Services;

namespace StartCheckerApp.Views;

public partial class RunnerDetailPage : ContentPage
{
    private readonly Runner _runner;
    private readonly HttpClient _httpClient;
    private readonly RaceDataService _raceDataService;
    private readonly RunnerDatabaseService _runnerDatabase;
    private readonly bool _isNewRunner;
    private const string DateFormat = "dd.MM.yyyy";
    private const string TimeFormat = "HH:mm:ss";


    public RunnerDetailPage(Runner runner, HttpClient httpClient, RaceDataService raceDataService, RunnerDatabaseService runnerDatabase)
    {
        InitializeComponent();
        _runner = runner;
        _httpClient = httpClient;
        _raceDataService = raceDataService;
        _runnerDatabase = runnerDatabase;

        _isNewRunner = _runner.ID == 0;

        FirstNameEntry.Text = _runner.FirstName;
        SurnameEntry.Text = _runner.Surname;
        CategoryEntry.Text = _runner.Category;
        RegistrationNumberEntry.Text = _runner.RegistrationNumber;
        SINumberEntry.Text = _runner.SINumber.ToString();
        StartDatePicker.Date = _runner.StartTime.ToLocalTime().Date;
        StartTimePicker.Time = _runner.StartTime.ToLocalTime().TimeOfDay;

        FullNameLabel.Text = _runner.FullName;

        ConfigureEditability();
    }

    private void ConfigureEditability()
    {
        bool readonlyFields = !_isNewRunner;

        FirstNameEntry.IsReadOnly = readonlyFields;
        SurnameEntry.IsReadOnly = readonlyFields;
        CategoryEntry.IsReadOnly = readonlyFields;
        RegistrationNumberEntry.IsReadOnly = readonlyFields;

        // Umožnìno vždy:
        SINumberEntry.IsReadOnly = false;
        StartDatePicker.IsEnabled = true;
        StartTimePicker.IsEnabled = true;
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        _runner.RegistrationNumber = RegistrationNumberEntry.Text;
        _runner.FirstName = FirstNameEntry.Text;
        _runner.Surname = SurnameEntry.Text;
        _runner.Category = CategoryEntry.Text;
        _runner.SINumber = int.Parse(SINumberEntry.Text);

        DateTime selectedDateTime = new DateTime(
            StartDatePicker.Date.Year,
            StartDatePicker.Date.Month,
            StartDatePicker.Date.Day,
            StartTimePicker.Time.Hours,
            StartTimePicker.Time.Minutes,
            StartTimePicker.Time.Seconds
        );

        _runner.StartTime = DateTime.SpecifyKind(selectedDateTime, DateTimeKind.Utc);

        if (_runner.ID == 0)
        {
            await _runnerDatabase.AddRunnerAsync(_runner);
        }
        else
        {
            await _runnerDatabase.UpdateRunnerAsync(_runner);
        }
        WeakReferenceMessenger.Default.Send(new RunnerUpdatedMessage(_runner));
        await Navigation.PopAsync();
    }

    private async void OnDNSClicked(object sender, EventArgs e)
    {
        _runner.DNS = true;
        _runner.StartPassage = null;
        _runner.Started = false;
        _runner.Late = false;
        _runner.StartFlag = false;
        _runner.LastUpdatedAt = DateTime.UtcNow;

        await _runnerDatabase.UpdateRunnerAsync(_runner);
        WeakReferenceMessenger.Default.Send(new RunnerUpdatedMessage(_runner));
        await Navigation.PopAsync();
    }

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

}
