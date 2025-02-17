using StartCheckerApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartCheckerApp
{
    public class RaceDataService
    {
        public List<Runner> Runners { get; private set; } = new List<Runner>();

        public void SetRunners(List<Runner> runners)
        {
            Runners = runners;
        }
        public void UpdateRunner(Runner updatedRunner)
        {
            int index = Runners.FindIndex(r => r.ID == updatedRunner.ID);
            if (index >= 0)
            {
                Runners[index] = updatedRunner;
            }
        }
    }
}
