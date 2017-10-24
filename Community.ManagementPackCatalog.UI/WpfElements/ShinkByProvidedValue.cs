// -----------------------------------------------------------------------
// <copyright file="ShinkByProvidedValue.cs">
// This is part of the Community Management Pack Catalog and licensed GPL v3; See https://github.com/mpcatalog/scom-community-catalog-ui/blob/master/LICENSE.
// </copyright>
// -----------------------------------------------------------------------

namespace Community.ManagementPackCatalog.UI.WpfElements
{
    using System;
    using System.Globalization;
    using System.Windows.Data;

    /// <summary>
    /// An extension of IValueConverter that is used to scale items in the UI
    /// This converter allows a binding to adjust value at runtime.
    /// </summary>
    public class ShinkByProvidedValue : IValueConverter
    {
        /// <summary>
        /// Gets or sets the value by which to shrink the element.
        /// </summary>
        public double ShinkByValue { get; set; }

        /// <summary>
        /// Handles the incoming convert by reducing the value by 40
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns>Returns an adjusted value.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((double)value) - ShinkByValue;
        }

        /// <summary>
        /// Converts values back to their original state.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns>The initial value, before conversion.</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((double)value) + ShinkByValue;
        }
    }
}