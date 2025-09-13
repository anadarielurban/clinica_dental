using System;
using System.Globalization;
using System.Windows.Data;

namespace DentalClinicWPF.Converters
{
    public class TimeSpanToIntConverter : IValueConverter
    {
        // Konwertuje TimeSpan na int (minuty)
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TimeSpan timeSpan)
                return (int)timeSpan.TotalMinutes;
            return 0;
        }

        // Konwertuje int (minuty) na TimeSpan
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int minutes && minutes > 0)
                return TimeSpan.FromMinutes(minutes);
            return TimeSpan.Zero;
        }
    }
}