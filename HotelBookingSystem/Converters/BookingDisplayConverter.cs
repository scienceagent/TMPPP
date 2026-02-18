using System;
using System.Globalization;
using System.Windows.Data;
using HotelBookingSystem.Models;

namespace HotelBookingSystem.Converters
{
     /// <summary>
     /// Converts a Booking to a display string (Id or Status) for binding.
     /// Use ConverterParameter "Id" or "Status".
     /// </summary>
     public class BookingDisplayConverter : IValueConverter
     {
          public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
          {
               if (value is Booking booking && parameter is string param)
               {
                    return param == "Id" ? booking.BookingId : booking.Status.ToString();
               }
               return "";
          }

          public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
          {
               throw new NotImplementedException();
          }
     }
}