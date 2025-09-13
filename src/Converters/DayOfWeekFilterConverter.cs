using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using DentalClinicWPF.Models; // Upewnij się, że referencja do Twojej klasy Appointment jest poprawna

namespace DentalClinicWPF.Converters
{
    public class DayOfWeekFilterConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is IEnumerable<Appointment> appointments && parameter is string dayParam)
            {
                // Ustalamy, który dzień tygodnia filtrować
                DayOfWeek targetDay = DayOfWeek.Monday;
                switch (dayParam.ToLower())
                {
                    case "monday":    targetDay = DayOfWeek.Monday;    break;
                    case "tuesday":   targetDay = DayOfWeek.Tuesday;   break;
                    case "wednesday": targetDay = DayOfWeek.Wednesday; break;
                    case "thursday":  targetDay = DayOfWeek.Thursday;  break;
                    case "friday":    targetDay = DayOfWeek.Friday;    break;
                    case "saturday":  targetDay = DayOfWeek.Saturday;  break;
                    case "sunday":    targetDay = DayOfWeek.Sunday;    break;
                }

                // Filtrujemy wizyty po dacie
                return appointments
                    .Where(a => a.AppointmentDate.DayOfWeek == targetDay)
                    .OrderBy(a => a.AppointmentDate.TimeOfDay) // sortujemy wg godziny startu
                    .ToList();
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}