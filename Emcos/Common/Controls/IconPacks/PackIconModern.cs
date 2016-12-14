using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace TMP.Wpf.Common.Icons
{
    /// <summary>
    /// Icon from the Material Design Icons project, <see><cref>https://materialdesignicons.com/</cref></see>.
    /// </summary>
    public class PackIconModern : Control
    {
        private static Lazy<IDictionary<PackIconModernKind, string>> _dataIndex;
        static PackIconModern()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PackIconModern), new FrameworkPropertyMetadata(typeof(PackIconModern)));
        }

        public PackIconModern()
        {
            _dataIndex = new Lazy<IDictionary<PackIconModernKind, string>>(PackIconModernDataFactory.Create);
        }

        public static readonly DependencyProperty KindProperty = DependencyProperty.Register(
            "Kind", typeof(PackIconModernKind), typeof(PackIconModern), new PropertyMetadata(default(PackIconModernKind), 
                KindPropertyChangedCallback));

        private static void KindPropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            ((PackIconModern)dependencyObject).UpdateData();
        }

        /// <summary>
        /// Gets or sets the icon to display.
        /// </summary>
        public PackIconModernKind Kind
        {
            get { return (PackIconModernKind)GetValue(KindProperty); }
            set { SetValue(KindProperty, value); }
        }

        private static readonly DependencyPropertyKey DataPropertyKey =
            DependencyProperty.RegisterReadOnly(
                "Data", typeof(string), typeof(PackIconModern),
                new PropertyMetadata(default(string)));

        // ReSharper disable once StaticMemberInGenericType
        public static readonly DependencyProperty DataProperty =
            DataPropertyKey.DependencyProperty;

        /// <summary>
        /// Gets the icon path data for the current <see cref="Kind"/>.
        /// </summary>
        [TypeConverter(typeof(GeometryConverter))]
        public string Data
        {
            get { return (string)GetValue(DataProperty); }
            private set { SetValue(DataPropertyKey, value); }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            UpdateData();
        }

        internal void UpdateData()
        {
            string data = null;
            if (Kind != PackIconModernKind.None && _dataIndex.Value != null)
                _dataIndex.Value.TryGetValue(Kind, out data);
            Data = data;
        }
    }
}
