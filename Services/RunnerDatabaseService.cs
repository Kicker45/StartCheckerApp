using SQLite;
using StartCheckerApp.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StartCheckerApp.Services
{
    public class RunnerDatabaseService
    {
        private readonly SQLiteAsyncConnection _database;


        public RunnerDatabaseService(SQLiteAsyncConnection database)
        {
            _database = database;
        }

        public async Task InitializeAsync()
        {
            await _database.CreateTableAsync<Runner>();
        }

        public async Task<List<Runner>> GetRunnersAsync()
        {
            return await _database.Table<Runner>().ToListAsync();
        }
        public async Task<Runner> GetRunnerByIdAsync(int runnerId)
        {
            return await _database.Table<Runner>().Where(r => r.ID == runnerId).FirstOrDefaultAsync();
        }


        public async Task AddRunnerAsync(Runner runner)
        {
            runner.LastUpdatedAt = DateTime.UtcNow;
            await _database.InsertAsync(runner);
        }

        public async Task UpdateRunnerAsync(Runner runner)
        {
            runner.LastUpdatedAt = DateTime.UtcNow;
            await _database.UpdateAsync(runner);
        }

        public async Task DeleteRunnerAsync(Runner runner)
        {
            await _database.DeleteAsync(runner);
        }

        public async Task DeleteAllRunnersAsync()
        {
            await _database.DeleteAllAsync<Runner>();
        }
        public async Task CreateIndexOnStartMinuteAsync()
        {
            await _database.ExecuteAsync("CREATE INDEX IF NOT EXISTS idx_runners_startminute ON Runner(StartTime);");
        }
        public async Task<List<Runner>> GetRunnersOrderedByStartMinuteAsync()
        {
            return await _database.QueryAsync<Runner>("SELECT * FROM Runner ORDER BY StartTime");
        }


    }
}
