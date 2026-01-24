using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;

namespace MagoLauncher.Presentation.Converters
{
    public class ActiveViewToForegroundConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string activeView && parameter is string targetView)
            {
                if (activeView == targetView)
                {
                    return Brushes.White; // Active color
                }
                return new SolidColorBrush(Color.Parse("#8b929a")); // Inactive color
            }
            return new SolidColorBrush(Color.Parse("#8b929a")); // Default to SteamTextSecondary
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
