//------------------------------------------------------------------------------
// Název souboru: Constants.cs
// Autor: Jan Nechanický
// Popis: Tento soubor obsahuje nastavení hodnot konstant pro celý projekt.
// Datum vytvoření: 1.2.2025
//------------------------------------------------------------------------------

using SQLite;

namespace StartCheckerApp
{
    public static class Constants
    {
        // URL REST služby (Android nepoužívá localhost)
        // Použijte http pro lokální nasazení. Pro produkci změňte na https.
        public static string URL = "35.159.141.232"; // AWS adresa, mění se při každém nasazení kontejneru

        // Schéma URL (http nebo https)
        public static string Scheme = "http"; // Používá se http pro lokální server

        // Port REST služby
        public static string Port = "8080"; // 5034 pro http; 8080 pro AWS docker

        // Kompletní URL REST služby
        public static string RestUrl = $"{Scheme}://{URL}:{Port}/api/StartList/";

        // Název lokální SQLite databáze
        public const string DatabaseFilename = "runners.db3";

        // Nastavení SQLite databáze
        public const SQLiteOpenFlags Flags =
            SQLiteOpenFlags.ReadWrite |    // Otevřít databázi v režimu čtení/zápisu
            SQLiteOpenFlags.Create |       // Vytvořit databázi, pokud neexistuje
            SQLiteOpenFlags.SharedCache;   // Povolit sdílenou mezipaměť pro vlákna

        // Cesta k SQLite databázi
        public static string DatabasePath =>
            Path.Combine(FileSystem.AppDataDirectory, DatabaseFilename);
    }

}
