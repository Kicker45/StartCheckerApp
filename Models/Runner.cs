using System;
using System.Text.Json.Serialization;

namespace StartCheckerApp.Models
{
    public class Runner
    {
        [JsonPropertyName("id")]
        public int ID { get; set; }

        [JsonPropertyName("registrationNumber")]
        public string RegistrationNumber { get; set; }

        [JsonPropertyName("firstName")]
        public string FirstName { get; set; }

        [JsonPropertyName("surname")]
        public string Surname { get; set; }

        [JsonPropertyName("category")]
        public string Category { get; set; }

        [JsonPropertyName("siNumber")]
        public int SINumber { get; set; }

        [JsonPropertyName("startTime")]
        public DateTime StartTime { get; set; }

        [JsonPropertyName("startPassage")]
        public DateTime? StartPassage { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("raceId")]
        public int RaceId { get; set; }
    }
}
