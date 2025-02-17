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

            // Napln�n� vstupn�ch pol� daty z�vodn�ka
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

            Close(_runner); // Vr�t� upraven�ho z�vodn�ka zp�t
        }

        private void OnCloseClicked(object sender, EventArgs e)
        {
            Close(null); // Zav�e popup bez zm�n
        }
    }
}
