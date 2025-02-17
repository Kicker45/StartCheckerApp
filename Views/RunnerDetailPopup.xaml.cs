using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Controls;
using StartCheckerApp.Models;
using System;

namespace StartCheckerApp.Views
{
    public partial class RunnerDetailPopup : Popup
    {
        private Runner _runner;

        public RunnerDetailPopup(Runner runner)
        {
            InitializeComponent();
            _runner = runner;

            // Naplnìní vstupních polí daty závodníka
            FirstNameEntry.Text = _runner.FirstName;
            SurnameEntry.Text = _runner.Surname;
            CategoryEntry.Text = _runner.Category;
            SINumberEntry.Text = _runner.SINumber.ToString();
            StartTimeEntry.Text = _runner.StartTime.ToString("HH:mm");
        }

        private void OnSaveClicked(object sender, EventArgs e)
        {
            _runner.FirstName = FirstNameEntry.Text;
            _runner.Surname = SurnameEntry.Text;
            _runner.Category = CategoryEntry.Text;
            _runner.SINumber = int.Parse(SINumberEntry.Text);
            _runner.StartTime = DateTime.Parse(StartTimeEntry.Text);

            Close(_runner); // Vrátí upraveného závodníka zpìt
        }

        private void OnCloseClicked(object sender, EventArgs e)
        {
            Close(null); // Zavøe popup bez zmìn
        }
    }
}
