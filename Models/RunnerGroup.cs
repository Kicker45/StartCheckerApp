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
