using SQLite;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace StartCheckerApp.Models
{
    public class Runner : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        [PrimaryKey]
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
        private DateTime? _startPassage;
        public DateTime? StartPassage
        {
            get => _startPassage;
            set { _startPassage = value; OnPropertyChanged(); }
        }

        [JsonPropertyName("startFlag")]
        private bool _startFlag;
        public bool StartFlag
        {
            get => _startFlag;
            set { _startFlag = value; OnPropertyChanged(); }
        }

        [JsonPropertyName("status")]
        private string _status;
        public string Status
        {
            get => _status;
            set { _status = value; OnPropertyChanged(); }
        }

        [JsonPropertyName("raceId")]
        public int RaceId { get; set; }

        public string FullName => $"{FirstName} {Surname}";

        [JsonIgnore]
        public string StartMinute => StartTime.ToString("HH:mm");

        [JsonIgnore]
        private DateTime _lastModified;
        public DateTime LastModified
        {
            get => _lastModified;
            set { _lastModified = value; OnPropertyChanged(); }
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
