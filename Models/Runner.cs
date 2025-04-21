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

        [JsonPropertyName("started")]
        private bool _started;
        public bool Started
        {
            get => _started;
            set
            {
                if (_started != value)
                {
                    _started = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(ComputedStatus)); // Nová vlastnost pro UI
                }
            }
        }


        [JsonPropertyName("dns")]
        private bool _dns;
        public bool DNS
        {
            get => _dns;
            set
            {
                if (_dns != value)
                {
                    _dns = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(ComputedStatus)); // Nová vlastnost pro UI
                }
            }
        }

        [JsonPropertyName("late")]
        private bool _late;
        public bool Late
        {
            get => _late;
            set
            {
                if (_late != value)
                {
                    _late = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(ComputedStatus)); // Nová vlastnost pro UI
                }
            }

        }
        //MAUI neumí multibinding proto je nutné použít toto pro změnu UI
        public string ComputedStatus
        {
            get
            {
                if (Started) return "Started";
                if (DNS) return "DNS";
                if (Late) return "Late";
                return "None";
            }
        }

        [JsonPropertyName("raceId")]
        public int RaceId { get; set; }

        public string FullName => $"{FirstName} {Surname}";

        [JsonIgnore]
        public string StartMinute => StartTime.ToString("HH:mm");

        [JsonPropertyName("lastUpdatedAt")]
        private DateTime _LastUpdatedAt;
        public DateTime LastUpdatedAt
        {
            get => _LastUpdatedAt;
            set { _LastUpdatedAt = value; OnPropertyChanged(); }
        }

        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
