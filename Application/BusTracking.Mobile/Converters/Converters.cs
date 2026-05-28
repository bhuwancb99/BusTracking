using System.Globalization;

namespace BusTracking.Mobile.Converters;

/// <summary>
/// Converts a Boolean value to its logical inverse for use in data binding.
/// </summary>
/// <remarks>Convert and ConvertBack both return the negation of the input when the input is a Boolean.
/// Non-Boolean inputs evaluate to false. The converter ignores targetType, parameter, and culture.</remarks>
public class InvertBoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is bool b && !b;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is bool b && !b;
}


/// <summary>
/// Converts a null value to false and a non-null value to true.
/// </summary>
/// <remarks>Intended for use in XAML bindings to map object presence to a boolean. Implements IValueConverter;
/// ConvertBack throws NotImplementedException and is not supported.</remarks>
public class NullToBoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is not null;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}


/// <summary>
/// Value converter that returns true when the input value is null and false otherwise.
/// </summary>
/// <remarks>Intended for one-way bindings; ConvertBack throws NotImplementedException.</remarks>
public class NullToVisibleConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is null;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

/// <summary>
/// Converts a status string to a boolean indicating whether the Start control should be visible; returns true when the
/// status is "Scheduled".
/// </summary>
/// <remarks>Performs a case-sensitive comparison against the literal "Scheduled" and returns false for null or
/// non-string inputs. The converter ignores the targetType and parameter. ConvertBack is not implemented and throws
/// NotImplementedException.</remarks>
public class StatusToStartVisibleConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is string s && s == "Scheduled";

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

/// <summary>
/// Converts a status string to a boolean that indicates whether the end element should be visible; returns true when
/// the status equals 'InProgress'.
/// </summary>
/// <remarks>Expects a string input and returns true only when it equals 'InProgress'. ConvertBack is not
/// implemented and throws NotImplementedException.</remarks>
public class StatusToEndVisibleConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is string s && s == "InProgress";

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

/// <summary>
/// Converts a boolean value to an eye icon filename for UI visibility: returns "eye_slash.png" when true and "eye.png"
/// when false.
/// </summary>
/// <remarks>Implements IValueConverter for data binding. ConvertBack is not implemented and throws
/// NotImplementedException. The converter ignores targetType, parameter, and culture.</remarks>
public class BoolToEyeIconConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is true ? "eye_slash.png" : "eye.png";

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

/// <summary>
/// Converts a boolean to a status Color: true maps to #059669 (green) and false maps to #dc2626 (red).
/// </summary>
/// <remarks>Implements IValueConverter for use in data binding. ConvertBack is not implemented and throws
/// NotImplementedException. The parameter and culture are ignored.</remarks>
public class BoolToStatusColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is true ? Color.FromArgb("#059669") : Color.FromArgb("#dc2626");

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

/// <summary>
/// Converts a platform name string (Mobile, Web, or other) to a Color: Mobile -> #2563eb, Web -> #059669, default ->
/// #64748b.
/// </summary>
/// <remarks>Accepts string inputs; null or unrecognized values return the default slate color. ConvertBack is not
/// implemented and throws NotImplementedException. Culture and targetType are ignored.</remarks>
public class PlatformToColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value?.ToString() switch
        {
            "Mobile" => Color.FromArgb("#2563eb"),
            "Web" => Color.FromArgb("#059669"),
            _ => Color.FromArgb("#64748b")
        };

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

/// <summary>
/// Converts an input to true when its string representation is not null or empty; otherwise false.
/// </summary>
/// <remarks>Intended for use as an IValueConverter in data binding scenarios. ConvertBack is not implemented and
/// throws NotImplementedException.</remarks>
public class StringToBoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => !string.IsNullOrEmpty(value?.ToString());

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
