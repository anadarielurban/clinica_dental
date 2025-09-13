using System;
using System.Globalization;
using System.Windows.Data;

namespace DentalClinicWPF.Converters
{
    public class TimeSpanToStringConverter : IValueConverter
    {
       
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TimeSpan timeSpan)
            {
                return timeSpan.ToString(@"hh\:mm");
            }
            return "-";
        }

       
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (TimeSpan.TryParseExact((string)value, @"hh\:mm", culture, out TimeSpan result))
            {
                return result;
            }
            return Binding.DoNothing;
        }
    }
}