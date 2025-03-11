using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace StartCheckerApp.Models
{
    public class RaceData
    {
        [JsonPropertyName("raceId")]
        public int RaceId { get; set; }

        [JsonPropertyName("raceName")]
        public string RaceName { get; set; }

        [JsonPropertyName("startList")]
        public List<Runner> StartList { get; set; } = new();

        [JsonPropertyName("lastUpdate")]
        public DateTime LastUpdate { get; set; }

        [JsonPropertyName("startFlag")]
        public bool StartFlag { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }
    }
}
