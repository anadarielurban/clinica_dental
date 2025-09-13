using System;
using System.Globalization;
using System.Windows.Data;

namespace DentalClinicWPF.Converters
{
    public class BooleanToYesNoConverter : IValueConverter
    {
        // Konwersja z bool na "Yes" lub "No"
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? "Yes" : "No";
            }
            return "No";
        }

        // Konwersja z "Yes"/"No" na bool (opcjonalnie)
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string str)
            {
                return str.Equals("Yes", StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }
    }
}