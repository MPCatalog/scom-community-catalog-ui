// -----------------------------------------------------------------------
// <copyright file="TextInputToVisibilityConverter.cs">
// This is part of the Community Management Pack Catalog and licensed GPL v3; See https://github.com/mpcatalog/scom-community-catalog-ui/blob/master/LICENSE.
// </copyright>
// -----------------------------------------------------------------------

// Demo from Andy L. & MissedMemo.com
// Used under the Code Project Open License (CPOL)  (http://www.codeproject.com/info/cpol10.aspx)

namespace Community.ManagementPackCatalog.UI.WpfElements
{
    using System;
    using System.Windows;
    using System.Windows.Data;

    /// <summary>
    /// Extension of <see cref="IMultiValueConverter"/> used to adjust element visibility based on content.
    /// </summary>
    public class TextInputToVisibilityConverter : IMultiValueConverter
    {
        /// <summary>
        /// This method converts that targeted TextInputBox into a visibility value based upon content.
        /// </summary>
        /// <param name="values"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns>The Visibility to use based on the parameter</returns>
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            // Always test MultiValueConverter inputs for non-null
            // (to avoid crash bugs for views in the designer)
            if (values[0] is bool && values[1] is bool)
            {
                bool hasText = !(bool)values[0];
                bool hasFocus = (bool)values[1];

                if (hasFocus || hasText)
                {
                    return Visibility.Collapsed;
                }
            }

            return Visibility.Visible;
        }

        /// <summary>
        /// The ConvertBack method is not used and will throw and exception if called.  The method is
        /// created as a requirement to the IMultiValueConverter interface.
        /// </summary>
        /// <param name="value">nThe parameter is not used.</param>
        /// <param name="targetTypes">The parameter is not used.</param>
        /// <param name="parameter">The parameter is not used.</param>
        /// <param name="culture">The parameter is not used.</param>
        /// <returns>A NotImplementedException, Don't call this</returns>
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}