//------------------------------------------------------------------------------
// Název souboru: RaceData.cs
// Autor: Jan Nechanický
// Popis: Tento soubor obsahuje definici datového modelu pro závod.
// Datum vytvoření: 1.2.2025
//------------------------------------------------------------------------------

using System.Text.Json.Serialization;

namespace StartCheckerApp.Models
{
    // Třída pro model závodu
    public class RaceData
    {
        // ID závodu
        [JsonPropertyName("raceId")]
        public int RaceId { get; set; }

        // Název závodu
        [JsonPropertyName("raceName")]
        public string RaceName { get; set; }

        // Startovní listina obsahující seznam závodníků
        [JsonPropertyName("startList")]
        public List<Runner> StartList { get; set; } = new();

        // Datum poslední aktualizace závodu
        [JsonPropertyName("lastUpdate")]
        public DateTime LastUpdate { get; set; }

        // Příznak, zda závod byl zahájen
        [JsonPropertyName("startFlag")]
        public bool StartFlag { get; set; }

        // Stav závodu (např. aktivní, dokončený)
        [JsonPropertyName("status")]
        public string Status { get; set; }
    }
}
