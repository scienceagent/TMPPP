using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace HotelBookingSystem.Converters
{
    public class IntToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int intValue)
            {
                int threshold = 0;
                if (parameter != null && int.TryParse(parameter.ToString(), out int p))
                {
                    threshold = p;
                }

                // If threshold is 0, we show if count > 0.
                // If I want to show if count == 0, I can use a different logic.
                // In my XAML, I used it for "Empty State": Visibility="{Binding AssignedRoomsCount, Converter={StaticResource IntToVisibilityConverter}, ConverterParameter=0}"
                // I want it to be Visible if count == 0.
                
                return intValue == threshold ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
