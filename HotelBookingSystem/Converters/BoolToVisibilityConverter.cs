// ============================================
// Converters/BoolToVisibilityConverter.cs
// ============================================
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace HotelBookingSystem.Converters
{
     /// <summary>
     /// Converter pentru transformarea bool în Visibility
     /// </summary>
     public class BoolToVisibilityConverter : IValueConverter
     {
          public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
          {
               if (value is bool boolValue)
               {
                    return boolValue ? Visibility.Visible : Visibility.Collapsed;
               }
               return Visibility.Collapsed;
          }

          public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
          {
               if (value is Visibility visibility)
               {
                    return visibility == Visibility.Visible;
               }
               return false;
          }
     }

     /// <summary>
     /// Converter invers - false = Visible, true = Collapsed
     /// </summary>
     public class InverseBoolToVisibilityConverter : IValueConverter
     {
          public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
          {
               if (value is bool boolValue)
               {
                    return boolValue ? Visibility.Collapsed : Visibility.Visible;
               }
               return Visibility.Visible;
          }

          public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
          {
               if (value is Visibility visibility)
               {
                    return visibility == Visibility.Collapsed;
               }
               return true;
          }
     }

     /// <summary>
     /// Converter pentru transformarea string-ului în Visibility
     /// Dacă string-ul este null sau gol, returnează Collapsed, altfel Visible
     /// </summary>
     public class StringToVisibilityConverter : IValueConverter
     {
          public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
          {
               if (value is string stringValue)
               {
                    return string.IsNullOrWhiteSpace(stringValue) ? Visibility.Collapsed : Visibility.Visible;
               }
               return Visibility.Collapsed;
          }

          public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
          {
               throw new NotImplementedException();
          }
     }

     /// <summary>
     /// Converter pentru transformarea numărului în Visibility
     /// Dacă numărul este >= 7, returnează Visible, altfel Collapsed
     /// </summary>
     public class NumberToVisibilityConverter : IValueConverter
     {
          public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
          {
               if (value is int intValue)
               {
                    return intValue >= 7 ? Visibility.Visible : Visibility.Collapsed;
               }
               return Visibility.Collapsed;
          }

          public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
          {
               throw new NotImplementedException();
          }
     }
}