namespace TMP.UI.Controls.WPF.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Documents;
    using System.Windows.Media;

    public class TextBlockHelper
    {
        public static string GetSelection(DependencyObject obj)
        {
            return (string)obj.GetValue(SelectionProperty);
        }

        public static void SetSelection(DependencyObject obj, string value)
        {
            obj.SetValue(SelectionProperty, value);
        }

        public static readonly DependencyProperty SelectionProperty =
            DependencyProperty.RegisterAttached("Selection", typeof(string), typeof(TextBlockHelper), 
                new PropertyMetadata(new PropertyChangedCallback(DoHighligthText)));

        public static Brush GetHighlightBackground(DependencyObject obj)
        {
            return (Brush)obj.GetValue(HighlightBackgroundProperty);
        }

        public static void SetHighlightBackground(DependencyObject obj, Brush value)
        {
            obj.SetValue(HighlightBackgroundProperty, value);
        }

        public static readonly DependencyProperty HighlightBackgroundProperty =
            DependencyProperty.RegisterAttached("HighlightBackground", typeof(Brush), typeof(TextBlockHelper),
                new PropertyMetadata(Brushes.Yellow, new PropertyChangedCallback(DoHighligthText)));

        public static Brush GetHighlightForeground(DependencyObject obj)
        {
            return (Brush)obj.GetValue(HighlightForegroundProperty);
        }

        public static void SetHighlightForeground(DependencyObject obj, Brush value)
        {
            obj.SetValue(HighlightForegroundProperty, value);
        }

        public static readonly DependencyProperty HighlightForegroundProperty =
            DependencyProperty.RegisterAttached("HighlightForeground", typeof(Brush), typeof(TextBlockHelper),
                new PropertyMetadata(Brushes.Black, new PropertyChangedCallback(DoHighligthText)));

        private static void DoHighligthText(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d == null)
                return;
            if (d is not TextBlock)
                throw new InvalidOperationException("Only valid for TextBlock");

            TextBlock txtBlock = d as TextBlock;
            string text = txtBlock.Text;
            if (string.IsNullOrEmpty(text))
                return;

            string highlightText = (string)d.GetValue(SelectionProperty);
            if (string.IsNullOrEmpty(highlightText))
                return;

            int index = text.IndexOf(highlightText, StringComparison.CurrentCultureIgnoreCase);
            if (index < 0)
                return;

            Brush backgroundBrush = (Brush)d.GetValue(HighlightBackgroundProperty);
            Brush foregroundBrush = (Brush)d.GetValue(HighlightForegroundProperty);

            string[] searchWords = highlightText.Split('|');
            string regularExpression = string.Empty;
            foreach (string s in searchWords)
            {
                if (regularExpression.Length > 0)
                    regularExpression += "|";

                // Use positive look ahead and positive look behind tags
                // so the break is before and after each word, so the
                // actual word is not removed by Regex.Split()
                regularExpression += string.Format("(?={0})|(?<={0})", s);
            }

            txtBlock.Inlines.Clear();

            string[] split = Regex.Split(text, regularExpression, RegexOptions.IgnoreCase);
            foreach (var str in split)
            {
                if (string.IsNullOrEmpty(str))
                    continue;
                Run run = new Run(str);
                if (Regex.IsMatch(str, regularExpression, RegexOptions.IgnoreCase))
                {
                    run.Background = backgroundBrush;
                    run.Foreground = foregroundBrush;
                }

                txtBlock.Inlines.Add(run);
            }

            /*while (true)
            {
                txtBlock.Inlines.AddRange(new Inline[]
                {
                    new Run(text[..index]),
                    new Run(text.Substring(index, highlightText.Length))
                    {



                        Background = backgroundBrush,
                        Foreground = foregroundBrush,
                    },
                });

                text = text[(index + highlightText.Length) ..];
                index = text.IndexOf(highlightText, StringComparison.CurrentCultureIgnoreCase);

                if (index < 0)
                {
                    txtBlock.Inlines.Add(new Run(text));
                    break;
                }
            }*/
        }
    }
}
