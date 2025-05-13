//------------------------------------------------------------------------------
// Název souboru: RunnerDatabaseService.cs
// Autor: Jan Nechanický
// Popis: Tento soubor obsahuje logiku pro přístup k SQLite databázi závodníků.
// Datum vytvoření: 1.4.2025
//------------------------------------------------------------------------------

using SQLite;
using StartCheckerApp.Models;

namespace StartCheckerApp.Services
{
    /// <summary>
    /// Poskytuje asynchronní CRUD operace nad tabulkou Runner v SQLite databázi.
    /// </summary>
    public class RunnerDatabaseService
    {
        private readonly SQLiteAsyncConnection _database; // Připojení k SQLite databázi

        /// <summary>
        /// Konstruktor přijímající instanci SQLiteAsyncConnection.
        /// </summary>
        public RunnerDatabaseService(SQLiteAsyncConnection database)
        {
            _database = database;
        }

        /// <summary>
        /// Vytvoří tabulku Runner, pokud neexistuje.
        /// </summary>
        public async Task InitializeAsync()
        {
            await _database.CreateTableAsync<Runner>();
        }

        /// <summary>
        /// Vrátí všechny závodníky z databáze.
        /// </summary>
        public async Task<List<Runner>> GetRunnersAsync()
        {
            return await _database.Table<Runner>().ToListAsync();
        }

        /// <summary>
        /// Vrátí závodníka podle jeho ID.
        /// </summary>
        public async Task<Runner> GetRunnerByIdAsync(int runnerId)
        {
            return await _database.Table<Runner>()
                                  .Where(r => r.ID == runnerId)
                                  .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Vloží nového závodníka do databáze a nastaví čas poslední aktualizace.
        /// </summary>
        public async Task AddRunnerAsync(Runner runner)
        {
            runner.LastUpdatedAt = DateTime.UtcNow;
            await _database.InsertAsync(runner);
        }

        /// <summary>
        /// Aktualizuje existujícího závodníka a nastaví čas poslední aktualizace.
        /// </summary>
        public async Task UpdateRunnerAsync(Runner runner)
        {
            runner.LastUpdatedAt = DateTime.UtcNow;
            await _database.UpdateAsync(runner);
        }

        /// <summary>
        /// Odstraní jednoho závodníka z databáze.
        /// </summary>
        public async Task DeleteRunnerAsync(Runner runner)
        {
            await _database.DeleteAsync(runner);
        }

        /// <summary>
        /// Odstraní všechny záznamy závodníků z tabulky.
        /// </summary>
        public async Task DeleteAllRunnersAsync()
        {
            await _database.DeleteAllAsync<Runner>();
        }

        /// <summary>
        /// Vytvoří index na sloupci StartTime pro rychlejší dotazy řazené podle startovní minuty.
        /// </summary>
        public async Task CreateIndexOnStartMinuteAsync()
        {
            await _database.ExecuteAsync(
                "CREATE INDEX IF NOT EXISTS idx_runners_startminute ON Runner(StartTime);");
        }

        /// <summary>
        /// Vrátí závodníky seřazené podle času startu.
        /// </summary>
        public async Task<List<Runner>> GetRunnersOrderedByStartMinuteAsync()
        {
            return await _database.QueryAsync<Runner>(
                "SELECT * FROM Runner ORDER BY StartTime");
        }
    }
}
