﻿/*
 *  Copyright © 2016, Russell Libby 
 */

using System;
using Template10.Behaviors;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace DevicePower.Converters
{
    /// <summary>
    /// Value converter that translates true to <see cref="EllipsisBehavior.Visibilities.Visible"/> and false to
    /// <see cref="EllipsisBehavior.Visibilities.Collapsed"/>, or the reverse if the parameter is "Reverse".
    /// </summary>
    public class EllipseToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// Convert boolean value to Visibility type.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <param name="targetType">The target type (expecting Visibility).</param>
        /// <param name="parameter">The object parameter.</param>
        /// <param name="language">The language.</param>
        /// <returns>The visibility state.</returns>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var state = System.Convert.ToBoolean(value);

            return (state ? EllipsisBehavior.Visibilities.Visible : EllipsisBehavior.Visibilities.Collapsed);
        }

        /// <summary>
        /// Convert visibility value to boolean type.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <param name="targetType">The target type (expecting Boolean).</param>
        /// <param name="parameter">The object parameter.</param>
        /// <param name="language">The language.</param>
        /// <returns>The visibility state.</returns>
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return (value is EllipsisBehavior.Visibilities && (EllipsisBehavior.Visibilities)value == EllipsisBehavior.Visibilities.Visible);
        }
    }
}

