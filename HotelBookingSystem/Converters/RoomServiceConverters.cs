using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using HotelBookingSystem.Composite;

namespace HotelBookingSystem.Converters
{
     public class RoomServicePriceConverter : IValueConverter
     {
          public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
          {
               if (value is RoomServiceComponent component)
                    return $"${component.GetPrice():F2}";
               return "$0.00";
          }

          public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
              => throw new NotImplementedException();
     }

     public class RoomServiceTypeLabelConverter : IValueConverter
     {
          public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
              => value is RoomServicePackage ? "PKG" : "LEAF";

          public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
              => throw new NotImplementedException();
     }

     public class RoomServiceTypeColorConverter : IValueConverter
     {
          private static readonly SolidColorBrush PackageColor = new(Color.FromRgb(0x00, 0x69, 0x5C));
          private static readonly SolidColorBrush LeafColor = new(Color.FromRgb(0x02, 0x77, 0xBD));

          public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
              => value is RoomServicePackage ? PackageColor : LeafColor;

          public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
              => throw new NotImplementedException();
     }
}