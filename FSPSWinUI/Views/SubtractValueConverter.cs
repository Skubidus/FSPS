using System;
using Microsoft.UI.Xaml.Data;

namespace FSPSWinUI.Views
{
    /// <summary>
    /// Subtracts a fixed value (parameter) from the bound width (value).
    /// Used to align the Path TextBox width to the Name TextBox minus the Browse button and spacing.
    /// </summary>
    public class SubtractValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is double width && double.TryParse(parameter?.ToString(), out double subtract))
            {
                return Math.Max(0, width - subtract);
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
