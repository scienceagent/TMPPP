using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace HotelBookingSystem.Converters
{
     public class ToastBackgroundConverter : IValueConverter
     {
          public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
          {
               if (value is ViewModels.ToastKind kind)
               {
                    return kind switch
                    {
                         ViewModels.ToastKind.Success => new SolidColorBrush(Color.FromRgb(0x1B, 0x5E, 0x20)),
                         ViewModels.ToastKind.Error => new SolidColorBrush(Color.FromRgb(0xB7, 0x1C, 0x1C)),
                         ViewModels.ToastKind.Warning => new SolidColorBrush(Color.FromRgb(0xE6, 0x51, 0x00)),
                         ViewModels.ToastKind.Info => new SolidColorBrush(Color.FromRgb(0x01, 0x57, 0x9B)),
                         _ => new SolidColorBrush(Color.FromRgb(0x37, 0x47, 0x4F))
                    };
               }
               return new SolidColorBrush(Colors.Gray);
          }

          public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
              => throw new NotImplementedException();
     }

     public class ToastIconConverter : IValueConverter
     {
          public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
          {
               if (value is ViewModels.ToastKind kind)
               {
                    return kind switch
                    {
                         ViewModels.ToastKind.Success => "✓",
                         ViewModels.ToastKind.Error => "✕",
                         ViewModels.ToastKind.Warning => "⚠",
                         ViewModels.ToastKind.Info => "ℹ",
                         _ => "•"
                    };
               }
               return "•";
          }

          public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
              => throw new NotImplementedException();
     }
}