// ============================================
// Converters/PriceFormatConverter.cs
// ============================================
using System;
using System.Globalization;
using System.Windows.Data;

namespace HotelBookingSystem.Converters
{
     /// <summary>
     /// Formatează prețul cu simbol monedă
     /// </summary>
     public class PriceFormatConverter : IValueConverter
     {
          public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
          {
               if (value is decimal price)
               {
                    return $"{price:N2} MDL";
               }
               return "0.00 MDL";
          }

          public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
          {
               throw new NotImplementedException();
          }
     }
}