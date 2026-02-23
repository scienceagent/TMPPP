using System;
using System.Globalization;
using System.Windows.Data;
using HotelBookingSystem.Models;

namespace HotelBookingSystem.Converters
{
     public class BookingDisplayConverter : IValueConverter
     {
          public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
          {
               if (value is Booking booking && parameter is string param)
               {
                    return param switch
                    {
                         "Id" => booking.BookingId,
                         "Type" => booking.GetType().Name.Replace("Booking", ""),
                         "CheckIn" => booking.CheckInDate.ToString("dd MMM yyyy"),
                         "CheckOut" => booking.CheckOutDate.ToString("dd MMM yyyy"),
                         "Status" => booking.Status.ToString(),
                         _ => string.Empty
                    };
               }
               return string.Empty;
          }

          public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
               => throw new NotImplementedException();
     }
}