//------------------------------------------------------------------------------
// N�zev souboru: RunnerDetailPage.xaml.cs
// Autor: Jan Nechanick�
// Popis: Tento soubor obsahuje obsluhu str�nky pro detail z�vodn�ka.
// Datum vytvo�en�: 1.4.2025
//------------------------------------------------------------------------------
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

        // Inicializace pol� s daty z�vodn�ka
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

    /// <summary>
    /// Nastav�, kter� pole mohou b�t editovateln� na z�klad� toho, zda se jedn� o nov�ho z�vodn�ka.
    /// </summary>
    private void ConfigureEditability()
    {
        bool readonlyFields = !_isNewRunner;

        FirstNameEntry.IsReadOnly = readonlyFields;
        SurnameEntry.IsReadOnly = readonlyFields;
        CategoryEntry.IsReadOnly = readonlyFields;
        RegistrationNumberEntry.IsReadOnly = readonlyFields;

        // Pole, kter� jsou v�dy povolena k �prav�
        SINumberEntry.IsReadOnly = false;
        StartDatePicker.IsEnabled = true;
        StartTimePicker.IsEnabled = true;
    }

    /// <summary>
    /// Ulo�� zm�ny proveden� na str�nce a aktualizuje datab�zi.
    /// </summary>
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

        _runner.StartTime = DateTime.SpecifyKind(selectedDateTime, DateTimeKind.Local).ToUniversalTime();

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

    /// <summary>
    /// Nastav� stav z�vodn�ka na DNS (Did Not Start) a aktualizuje datab�zi.
    /// </summary>
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

    /// <summary>
    /// Zru�� zm�ny a vr�t� u�ivatele na p�edchoz� str�nku.
    /// </summary>
    private async void OnCancelClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}
