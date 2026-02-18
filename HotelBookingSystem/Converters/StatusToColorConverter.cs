// ============================================
// Converters/StatusToColorConverter.cs
// ============================================
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using HotelBookingSystem.Models;

namespace HotelBookingSystem.Converters
{
     /// <summary>
     /// Convertește status-ul booking-ului în culoare
     /// </summary>
     public class StatusToColorConverter : IValueConverter
     {
          public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
          {
               if (value is BookingStatus status)
               {
                    return status switch
                    {
                         BookingStatus.Pending => new SolidColorBrush(Colors.Orange),
                         BookingStatus.Confirmed => new SolidColorBrush(Colors.Green),
                         BookingStatus.Cancelled => new SolidColorBrush(Colors.Red),
                         BookingStatus.Completed => new SolidColorBrush(Colors.Blue),
                         _ => new SolidColorBrush(Colors.Gray)
                    };
               }
               return new SolidColorBrush(Colors.Gray);
          }

          public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
          {
               throw new NotImplementedException();
          }
     }
}