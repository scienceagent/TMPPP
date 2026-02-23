using System;
using System.Globalization;
using System.Windows.Data;

namespace HotelBookingSystem.Converters
{
     /// <summary>Formats a decimal price value as a USD currency string.</summary>
     public class PriceFormatConverter : IValueConverter
     {
          private static readonly CultureInfo _enUs = CultureInfo.GetCultureInfo("en-US");

          public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
          {
               if (value is decimal price)
                    return price.ToString("C", _enUs);
               return "$0.00";
          }

          public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
               => throw new NotImplementedException();
     }
}