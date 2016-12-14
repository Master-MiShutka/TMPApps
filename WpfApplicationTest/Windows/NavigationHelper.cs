using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WpfApplicationTest.Windows.Helpers
{
    /// <summary>
    /// Provides helper function for navigation.
    /// </summary>
    public static class NavigationHelper
    {
        /// <summary>
        /// Tries to cast specified value to a uri. Either a uri or string input is accepted.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Uri ToUri(object value)
        {
            var uri = value as Uri;
            if (uri == null)
            {
                var uriString = value as string;
                if (uriString == null || !Uri.TryCreate(uriString, UriKind.RelativeOrAbsolute, out uri))
                {
                    return null; // no valid uri found
                }
            }
            return uri;
        }
    }
}
