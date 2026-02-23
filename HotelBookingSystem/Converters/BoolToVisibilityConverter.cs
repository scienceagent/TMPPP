using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace HotelBookingSystem.Converters
{
     public class BoolToVisibilityConverter : IValueConverter
     {
          public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
          {
               if (value is bool b)
                    return b ? Visibility.Visible : Visibility.Collapsed;
               return Visibility.Collapsed;
          }

          public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
          {
               if (value is Visibility v)
                    return v == Visibility.Visible;
               return false;
          }
     }
     public class InverseBoolToVisibilityConverter : IValueConverter
     {
          public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
          {
               if (value is bool b)
                    return b ? Visibility.Collapsed : Visibility.Visible;
               return Visibility.Visible;
          }

          public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
          {
               if (value is Visibility v)
                    return v == Visibility.Collapsed;
               return true;
          }
     }
     public class StringToVisibilityConverter : IValueConverter
     {
          public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
          {
               if (value is string s)
                    return string.IsNullOrWhiteSpace(s) ? Visibility.Collapsed : Visibility.Visible;
               return Visibility.Collapsed;
          }

          public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
               => throw new NotImplementedException();
     }
     public class NumberToVisibilityConverter : IValueConverter
     {
          public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
          {
               if (value is int n)
                    return n >= 7 ? Visibility.Visible : Visibility.Collapsed;
               return Visibility.Collapsed;
          }

          public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
               => throw new NotImplementedException();
     }
}