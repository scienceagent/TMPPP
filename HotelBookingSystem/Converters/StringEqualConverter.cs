using System;
using System.Globalization;
using System.Windows.Data;

namespace HotelBookingSystem.Converters
{
     // Used by strategy radio buttons:
     // IsChecked="{Binding StrategyCtrl.SelectedStrategyName,
     //              Converter={StaticResource StringEqualConverter},
     //              ConverterParameter='Standard Rate', Mode=TwoWay}"
     public sealed class StringEqualConverter : IValueConverter
     {
          public object Convert(object value, Type targetType,
                                object parameter, CultureInfo culture)
              => value is string s && parameter is string p && s == p;

          public object ConvertBack(object value, Type targetType,
                                    object parameter, CultureInfo culture)
          {
               // Only write back when the RadioButton becomes CHECKED
               if (value is bool b && b && parameter is string p)
                    return p;
               return Binding.DoNothing;
          }
     }
}