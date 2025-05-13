//------------------------------------------------------------------------------
// Název souboru: Runner.cs
// Autor: Jan Nechanický
// Popis: Tento soubor obsahuje definici datového modelu pro závodníka.
// Datum vytvoření: 1.2.2025
//------------------------------------------------------------------------------
using SQLite;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace StartCheckerApp.Models
{
    // Třída pro model závodníka, implementující INotifyPropertyChanged pro aktualizaci UI při změně vlastností
    public class Runner : INotifyPropertyChanged
    {
        // Událost pro sledování změn vlastností
        public event PropertyChangedEventHandler PropertyChanged;

        // Primární klíč pro SQLite tabulku
        [PrimaryKey]
        [JsonPropertyName("id")]
        public int ID { get; set; }

        // Registrační číslo závodníka
        [JsonPropertyName("registrationNumber")]
        public string RegistrationNumber { get; set; }

        // Křestní jméno závodníka
        [JsonPropertyName("firstName")]
        public string FirstName { get; set; }

        // Příjmení závodníka
        [JsonPropertyName("surname")]
        public string Surname { get; set; }

        // Kategorie závodníka
        [JsonPropertyName("category")]
        public string Category { get; set; }

        // Číslo SI čipu závodníka
        [JsonPropertyName("siNumber")]
        public int SINumber { get; set; }

        // Startovní čas závodníka
        [JsonPropertyName("startTime")]
        public DateTime StartTime { get; set; }

        // Čas, kdy závodník projde startem (StartPassage)
        [JsonPropertyName("startPassage")]
        private DateTime? _startPassage;
        public DateTime? StartPassage
        {
            get => _startPassage;
            set { _startPassage = value; OnPropertyChanged(); } // Změna vyvolá PropertyChanged
        }

        // Příznak, zda závodník odstartoval (StartFlag)
        [JsonPropertyName("startFlag")]
        private bool _startFlag;
        public bool StartFlag
        {
            get => _startFlag;
            set { _startFlag = value; OnPropertyChanged(); } // Změna vyvolá PropertyChanged
        }

        // Příznak, zda závodník začal závod (Started)
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
                    OnPropertyChanged(nameof(ComputedStatus)); // Aktualizace statusu pro UI
                }
            }
        }

        // Příznak, zda závodník je označen jako DNS (Did Not Start)
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
                    OnPropertyChanged(nameof(ComputedStatus)); // Aktualizace statusu pro UI
                }
            }
        }

        // Příznak, zda závodník je pozdě (Late)
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
                    OnPropertyChanged(nameof(ComputedStatus)); // Aktualizace statusu pro UI
                }
            }
        }

        // Vlastnost, která vrací status závodníka (Started, DNS, Late, None)
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

        // ID závodu, ke kterému závodník patří
        [JsonPropertyName("raceId")]
        public int RaceId { get; set; }

        // Úplné jméno závodníka (křestní jméno + příjmení) pro zobrazení v UI
        public string FullName => $"{FirstName} {Surname}";

        // Startovní minuta ve formátu HH:mm
        [JsonIgnore]
        public string StartMinute => StartTime.ToLocalTime().ToString("HH:mm");

        // Čas poslední změny pro synchronizaci
        [JsonPropertyName("lastUpdatedAt")]
        private DateTime _LastUpdatedAt;
        public DateTime LastUpdatedAt
        {
            get => _LastUpdatedAt;
            set { _LastUpdatedAt = value; OnPropertyChanged(); }
        }

        // Událost pro PropertyChanged, která informuje o změně hodnoty vlastnosti
        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
