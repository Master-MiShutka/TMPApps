using System;
using System.Windows;
using System.Windows.Media;

namespace TMP.Wpf.CommonControls.Dialogs
{
    /// <summary>
    /// Внутренний элемент управления, представляющий диалог хода события
    /// </summary>
    public partial class ProgressDialog : BaseTMPDialog
    {
        internal ProgressDialog(TMPWindow parentWindow, TMPDialogSettings settings)
            : base(parentWindow, settings)
        {
            InitializeComponent();

            if (parentWindow.TMPDialogOptions.ColorScheme == TMPDialogColorScheme.Theme)
            {
                try
                {
                    ProgressBarForeground = ThemeManager.GetResourceFromAppStyle(parentWindow, "AccentColorBrush") as Brush;
                }
                catch (Exception) { }
            }
            else
            {
                ProgressBarForeground = Brushes.White;
            }
        }

        internal ProgressDialog(TMPWindow parentWindow)
            : this(parentWindow, null)
        { }

        public static readonly DependencyProperty ProgressBarForegroundProperty = DependencyProperty.Register("ProgressBarForeground", typeof(Brush), typeof(ProgressDialog), new PropertyMetadata(default(string)));
        public static readonly DependencyProperty MessageProperty = DependencyProperty.Register("Message", typeof(string), typeof(ProgressDialog), new PropertyMetadata(default(string)));

        public static readonly DependencyProperty IsCancelableProperty = DependencyProperty.Register("IsCancelable", typeof(bool), typeof(ProgressDialog), new PropertyMetadata(default(bool), (s, e) =>
        {
            ((ProgressDialog)s).PART_NegativeButton.Visibility = (bool)e.NewValue ? Visibility.Visible : Visibility.Hidden;
        }));

        public static readonly DependencyProperty NegativeButtonTextProperty = DependencyProperty.Register("NegativeButtonText", typeof(string), typeof(ProgressDialog), new PropertyMetadata("Cancel"));

        public string Message
        {
            get => (string)GetValue(MessageProperty);
            set => SetValue(MessageProperty, value);
        }

        public bool IsCancelable
        {
            get => (bool)GetValue(IsCancelableProperty);
            set => SetValue(IsCancelableProperty, value);
        }

        public string NegativeButtonText
        {
            get => (string)GetValue(NegativeButtonTextProperty);
            set => SetValue(NegativeButtonTextProperty, value);
        }

        public Brush ProgressBarForeground
        {
            get => (Brush)GetValue(ProgressBarForegroundProperty);
            set => SetValue(ProgressBarForegroundProperty, value);
        }
    }
}