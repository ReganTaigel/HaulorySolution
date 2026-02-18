using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace Haulory.Mobile.Converters;

public class StringNotEmptyConverter : IValueConverter
{
    #region Convert

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        // Returns true if string is not null/empty/whitespace
        return !string.IsNullOrWhiteSpace(value as string);
    }

    #endregion

    #region ConvertBack

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        // One-way binding only
        throw new NotImplementedException();
    }

    #endregion
}
