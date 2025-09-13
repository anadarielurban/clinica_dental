using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace DentalClinicWPF.Converters
{
    /// <summary>
    /// Konwerter przekształcający status wizyty na odpowiedni kolor.
    /// </summary>
    public class StatusToColorConverter : IValueConverter
    {
        /// <summary>
        /// Przekształca status wizyty na odpowiedni kolor.
        /// </summary>
        /// <param name="value">Status wizyty (np. "Confirmed", "Pending", "Cancelled").</param>
        /// <param name="targetType">Typ docelowy (zwykle Brush).</param>
        /// <param name="parameter">Dodatkowy parametr (nieużywany).</param>
        /// <param name="culture">Informacje o kulturze.</param>
        /// <returns>Brush odpowiadający statusowi.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string status)
            {
                switch (status.ToLower())
                {
                    case "confirmed":
                        return Brushes.Green;
                    case "pending":
                        return Brushes.Orange;
                    case "cancelled":
                        return Brushes.Red;
                    default:
                        return Brushes.Gray; // Domyślny kolor dla nieznanych statusów
                }
            }

            return Brushes.Gray; // Domyślny kolor, jeśli wartość nie jest stringiem
        }

        /// <summary>
        /// Metoda niezaimplementowana, ponieważ konwersja wsteczna nie jest wymagana.
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}