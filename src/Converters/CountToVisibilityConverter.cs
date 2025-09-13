using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace DentalClinicWPF.Converters
{
    /// <summary>
    /// Konwerter zamienia liczbę na Visibility:
    /// Jeśli parametrem jest "Zero", to 0 -> Visible, inna liczba -> Collapsed.
    /// Możesz rozszerzyć logikę, jeśli potrzebujesz innych warunków.
    /// </summary>
    public class CountToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Sprawdzamy, czy 'value' to int
            if (value is int count)
            {
                // Jeżeli parametr to "Zero"
                // to 0 => Visible, inna liczba => Collapsed
                if (parameter?.ToString() == "Zero")
                {
                    return (count == 0) ? Visibility.Visible : Visibility.Collapsed;
                }

                // Jeśli nie używasz parametru lub chcesz innej logiki,
                // możesz tutaj wpisać własne warunki
            }

            // Domyślnie ukrywamy kontrolkę, jeśli nie spełniono warunków
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Nie implementujemy odwrotnej konwersji
            throw new NotImplementedException();
        }
    }
}