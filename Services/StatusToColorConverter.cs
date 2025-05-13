//------------------------------------------------------------------------------
// Název souboru: StatusToColorConverter.cs
// Autor: Jan Nechanický
// Popis: Tento soubor obsahuje logiku pro změnu barev závodníků ve startovní listině podle jejich statusu.
// Datum vytvoření: 1.4.2025
//------------------------------------------------------------------------------
using System.Globalization;

namespace StartCheckerApp.Services
{
    public class StatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string status)
            {
                return status switch
                {
                    "Started" => Colors.LightGreen,
                    "DNS" => Colors.Yellow,
                    "Late" => Colors.Orange,
                    _ => Colors.White
                };
            }

            return Colors.White;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
