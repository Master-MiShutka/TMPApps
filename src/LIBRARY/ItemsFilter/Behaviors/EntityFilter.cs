namespace ItemsFilter.Behaviors
{
    using Microsoft.Xaml.Behaviors;
    using System;
    using System.Text.RegularExpressions;
    using System.Windows;
    using System.Windows.Controls;

#nullable enable annotations
    public class EntityFilter : Behavior<ListBox>
    {
        public string? FilterText
        {
            get => (string)this.GetValue(FilterTextProperty);
            set => this.SetValue(FilterTextProperty, value);
        }

        /// <summary>
        /// Identifies the <see cref="FilterText"/> dependency property
        /// </summary>
        public static readonly DependencyProperty FilterTextProperty =
            DependencyProperty.Register(nameof(FilterText), typeof(string), typeof(EntityFilter),
                new FrameworkPropertyMetadata(null, (sender, e) => ((EntityFilter)sender).FilterText_Changed((string)e.NewValue)));

        private void FilterText_Changed(string? value)
        {
            var listBox = this.AssociatedObject;
            if (listBox == null)
                return;

            listBox.Items.Filter = BuildFilter(value);
        }

        public static Predicate<object>? BuildFilter(string value)
        {
            if (string.IsNullOrEmpty(value))
                return null;

            value = value.Trim();

            try
            {
                var regex = new Regex(value, RegexOptions.IgnoreCase | RegexOptions.Singleline);
                return item => regex.Match(item.ToString()).Success;
            }
            catch (ArgumentException)
            {
            }

            try
            {
                var regex = new Regex(value.Replace(@"\", @"\\", StringComparison.Ordinal), RegexOptions.IgnoreCase | RegexOptions.Singleline);
                return item => regex.Match(item.ToString()).Success;
            }
            catch (ArgumentException)
            {
            }

            return null;
        }

        protected override void OnAttached()
        {
            this.FilterText_Changed(FilterText);
        }
    }
}
