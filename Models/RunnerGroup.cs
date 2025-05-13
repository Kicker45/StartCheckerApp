//------------------------------------------------------------------------------
// Název souboru: RunnerGroup.cs
// Autor: Jan Nechanický
// Popis: Tento soubor obsahuje definici modelu pro skupinu závodníku pro zobrazení v UI po startovních minutách.
// Datum vytvoření: 1.4.2025
//------------------------------------------------------------------------------

using StartCheckerApp.Models;
using System.Collections.ObjectModel;

public class RunnerGroup : ObservableCollection<Runner>
{
    public string StartTime { get; private set; }

    public RunnerGroup(string startTime, IEnumerable<Runner> runners) : base(runners)
    {
        StartTime = startTime;
    }
}
