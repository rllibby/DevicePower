using System;
using Windows.UI.Xaml.Data;

namespace DevicePower.Converters
{
    /// <summary>
    /// Value converter that translates doubles to int.
    /// </summary>
    public class DoubleToIntConverter : IValueConverter
    {
        /// <summary>
        /// Convert double value to integer type.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <param name="targetType">The target type (expecting double).</param>
        /// <param name="parameter">The object parameter.</param>
        /// <param name="language">The language.</param>
        /// <returns>The integer value.</returns>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return System.Convert.ToInt32(value);
        }

        /// <summary>
        /// Convert integer value to double type.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <param name="targetType">The target type (expecting int).</param>
        /// <param name="parameter">The object parameter.</param>
        /// <param name="language">The language.</param>
        /// <returns>The double value.</returns>
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return System.Convert.ToDouble(value);
        }
    }
}
