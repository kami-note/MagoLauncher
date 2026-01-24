using Avalonia.Data.Converters;
using Avalonia.Media.TextFormatting;
using Avalonia.Media;
using System;
using System.Globalization;

namespace MagoLauncher.Presentation.Converters
{
    public class ActiveViewToUnderlineConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string activeView && parameter is string targetView)
            {
                if (activeView == targetView)
                {
                    return TextDecorations.Underline;
                }
            }
            return null;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
