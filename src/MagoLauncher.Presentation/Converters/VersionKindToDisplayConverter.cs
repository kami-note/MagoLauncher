using System;
using System.Globalization;
using Avalonia.Data.Converters;
using MagoLauncher.Domain.Enums;

namespace MagoLauncher.Presentation.Converters;

/// <summary>
/// Converts VersionKind enum to display string (e.g. All -> "Todos") for the sidebar filter ComboBox.
/// </summary>
public class VersionKindToDisplayConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not VersionKind kind)
            return value?.ToString() ?? "";

        return kind switch
        {
            VersionKind.All => "Todos",
            VersionKind.Stable => "Stable",
            VersionKind.Snapshot => "Snapshot",
            VersionKind.Especial => "Especial",
            VersionKind.Beta => "Beta",
            VersionKind.Alpha => "Alpha",
            VersionKind.Modpack => "Modpack",
            _ => kind.ToString()
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
