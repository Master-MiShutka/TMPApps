// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under the GNU LGPL (for details please see \doc\license.txt)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Data;
using System.Globalization;

namespace ICSharpCode.TreeView
{
	public class CollapsedWhenFalse : MarkupExtension, IValueConverter
	{
		public static CollapsedWhenFalse Instance = new CollapsedWhenFalse();

		public override object ProvideValue(IServiceProvider serviceProvider)
		{
			return Instance;
		}

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return (bool)value ? Visibility.Visible : Visibility.Collapsed;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}

	public class ExpandCollapseToggleMargin : IMultiValueConverter
	{
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            double height = (double)values[0];
            if (Double.IsNaN(height) || height == 0) return new Thickness(1d);
            bool expanded = (bool)values[1];
            Thickness margin = new Thickness(1d);
            if (expanded)
            {
                double borderMargin = 0;
                double borderThickness = 1;
                double lineThickness = 2;

                double top = (height - borderThickness * 2 - borderMargin * 2 - lineThickness) / 2;
                return new Thickness(1, top, 1, top);
            }
            return margin;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
