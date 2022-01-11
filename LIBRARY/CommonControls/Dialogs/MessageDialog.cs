using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace TMP.Wpf.CommonControls.Dialogs
{
    /// <summary>
    /// Внутренний элемент управления, представляющий диалог сообщения. Использовать вместо TMPWindow.ShowMessage!
    /// </summary>
    public partial class MessageDialog : BaseTMPDialog
    {
        internal MessageDialog(TMPWindow parentWindow)
            : this(parentWindow, null)
        {
        }

        internal MessageDialog(TMPWindow parentWindow, TMPDialogSettings settings)
            : base(parentWindow, settings)
        {
            InitializeComponent();

            PART_MessageScrollViewer.Height = DialogSettings.MaximumBodyHeight;
        }

        internal Task<MessageDialogResult> WaitForButtonPressAsync()
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                SetDefaultButton();
            }));

            TaskCompletionSource<MessageDialogResult> tcs = new TaskCompletionSource<MessageDialogResult>();

            RoutedEventHandler negativeHandler = null;
            KeyEventHandler negativeKeyHandler = null;

            RoutedEventHandler affirmativeHandler = null;
            KeyEventHandler affirmativeKeyHandler = null;

            RoutedEventHandler firstAuxHandler = null;
            KeyEventHandler firstAuxKeyHandler = null;

            RoutedEventHandler secondAuxHandler = null;
            KeyEventHandler secondAuxKeyHandler = null;

            Action cleanUpHandlers = () =>
            {
                PART_NegativeButton.Click -= negativeHandler;
                PART_AffirmativeButton.Click -= affirmativeHandler;
                PART_FirstAuxiliaryButton.Click -= firstAuxHandler;
                PART_SecondAuxiliaryButton.Click -= secondAuxHandler;

                PART_NegativeButton.KeyDown -= negativeKeyHandler;
                PART_AffirmativeButton.KeyDown -= affirmativeKeyHandler;
                PART_FirstAuxiliaryButton.KeyDown -= firstAuxKeyHandler;
                PART_SecondAuxiliaryButton.KeyDown -= secondAuxKeyHandler;
            };

            negativeKeyHandler = (sender, e) =>
            {
                if (e.Key == Key.Enter)
                {
                    cleanUpHandlers();

                    tcs.TrySetResult(MessageDialogResult.Negative);
                }
            };

            affirmativeKeyHandler = (sender, e) =>
            {
                if (e.Key == Key.Enter)
                {
                    cleanUpHandlers();

                    tcs.TrySetResult(MessageDialogResult.Affirmative);
                }
            };

            firstAuxKeyHandler = (sender, e) =>
            {
                if (e.Key == Key.Enter)
                {
                    cleanUpHandlers();

                    tcs.TrySetResult(MessageDialogResult.FirstAuxiliary);
                }
            };

            secondAuxKeyHandler = (sender, e) =>
            {
                if (e.Key == Key.Enter)
                {
                    cleanUpHandlers();

                    tcs.TrySetResult(MessageDialogResult.SecondAuxiliary);
                }
            };

            negativeHandler = (sender, e) =>
            {
                cleanUpHandlers();

                tcs.TrySetResult(MessageDialogResult.Negative);

                e.Handled = true;
            };

            affirmativeHandler = (sender, e) =>
            {
                cleanUpHandlers();

                tcs.TrySetResult(MessageDialogResult.Affirmative);

                e.Handled = true;
            };

            firstAuxHandler = (sender, e) =>
            {
                cleanUpHandlers();

                tcs.TrySetResult(MessageDialogResult.FirstAuxiliary);

                e.Handled = true;
            };

            secondAuxHandler = (sender, e) =>
            {
                cleanUpHandlers();

                tcs.TrySetResult(MessageDialogResult.SecondAuxiliary);

                e.Handled = true;
            };

            PART_NegativeButton.KeyDown += negativeKeyHandler;
            PART_AffirmativeButton.KeyDown += affirmativeKeyHandler;
            PART_FirstAuxiliaryButton.KeyDown += firstAuxKeyHandler;
            PART_SecondAuxiliaryButton.KeyDown += secondAuxKeyHandler;

            PART_NegativeButton.Click += negativeHandler;
            PART_AffirmativeButton.Click += affirmativeHandler;
            PART_FirstAuxiliaryButton.Click += firstAuxHandler;
            PART_SecondAuxiliaryButton.Click += secondAuxHandler;

            return tcs.Task;
        }

        internal Task<MessageDialogResult> SetupOkDialog()
        {
            SetDefaultButton();

            RoutedEventHandler affirmativeHandler = null;
            KeyEventHandler affirmativeKeyHandler = null;

            Action cleanUpHandlers = () =>
            {
                PART_AffirmativeButton.Click -= affirmativeHandler;

                PART_AffirmativeButton.KeyDown -= affirmativeKeyHandler;
            };

            TaskCompletionSource<MessageDialogResult> tcs = new TaskCompletionSource<MessageDialogResult>();

            affirmativeKeyHandler = (sender, e) =>
            {
                if (e.Key == Key.Enter)
                {
                    cleanUpHandlers();
                    Close();
                    tcs.TrySetResult(MessageDialogResult.Affirmative);
                }
            };

            affirmativeHandler = (sender, e) =>
            {
                cleanUpHandlers();
                Close();
                tcs.TrySetResult(MessageDialogResult.Affirmative);
                e.Handled = true;
            };

            PART_AffirmativeButton.KeyDown += affirmativeKeyHandler;
            PART_AffirmativeButton.Click += affirmativeHandler;

            return tcs.Task;
        }

        public static readonly DependencyProperty MessageProperty = DependencyProperty.Register("Message", typeof(string), typeof(MessageDialog), new PropertyMetadata(default(string)));
        public static readonly DependencyProperty AffirmativeButtonTextProperty = DependencyProperty.Register("AffirmativeButtonText", typeof(string), typeof(MessageDialog), new PropertyMetadata("OK"));
        public static readonly DependencyProperty NegativeButtonTextProperty = DependencyProperty.Register("NegativeButtonText", typeof(string), typeof(MessageDialog), new PropertyMetadata("Cancel"));
        public static readonly DependencyProperty FirstAuxiliaryButtonTextProperty = DependencyProperty.Register("FirstAuxiliaryButtonText", typeof(string), typeof(MessageDialog), new PropertyMetadata("Cancel"));
        public static readonly DependencyProperty SecondAuxiliaryButtonTextProperty = DependencyProperty.Register("SecondAuxiliaryButtonText", typeof(string), typeof(MessageDialog), new PropertyMetadata("Cancel"));

        public static readonly DependencyProperty ButtonStyleProperty = DependencyProperty.Register("ButtonStyle", typeof(MessageDialogStyle), typeof(MessageDialog), new PropertyMetadata(MessageDialogStyle.Affirmative, new PropertyChangedCallback((s, e) =>
        {
            MessageDialog md = (MessageDialog)s;

            SetButtonState(md);
        })));

        internal void SetDefaultButton()
        {
            this.Focus();

            //kind of acts like a selective 'IsDefault' mechanism.
            switch (this.ButtonStyle)
            {
                case MessageDialogStyle.Affirmative:
                    PART_AffirmativeButton.Focus();
                    break;

                case MessageDialogStyle.AffirmativeAndNegative:
                case MessageDialogStyle.AffirmativeAndNegativeAndDoubleAuxiliary:
                case MessageDialogStyle.AffirmativeAndNegativeAndSingleAuxiliary:
                    PART_NegativeButton.Focus();
                    break;
            }
        }

        private static void SetButtonState(MessageDialog md)
        {
            if (md.PART_AffirmativeButton == null)
                return;

            switch (md.ButtonStyle)
            {
                case MessageDialogStyle.Affirmative:
                    {
                        md.PART_AffirmativeButton.Visibility = Visibility.Visible;
                        md.PART_NegativeButton.Visibility = Visibility.Collapsed;
                        md.PART_FirstAuxiliaryButton.Visibility = Visibility.Collapsed;
                        md.PART_SecondAuxiliaryButton.Visibility = Visibility.Collapsed;
                    }
                    break;

                case MessageDialogStyle.AffirmativeAndNegativeAndSingleAuxiliary:
                case MessageDialogStyle.AffirmativeAndNegativeAndDoubleAuxiliary:
                case MessageDialogStyle.AffirmativeAndNegative:
                    {
                        md.PART_AffirmativeButton.Visibility = Visibility.Visible;
                        md.PART_NegativeButton.Visibility = Visibility.Visible;

                        if (md.ButtonStyle == MessageDialogStyle.AffirmativeAndNegativeAndSingleAuxiliary || md.ButtonStyle == MessageDialogStyle.AffirmativeAndNegativeAndDoubleAuxiliary)
                        {
                            md.PART_FirstAuxiliaryButton.Visibility = Visibility.Visible;
                        }

                        if (md.ButtonStyle == MessageDialogStyle.AffirmativeAndNegativeAndDoubleAuxiliary)
                        {
                            md.PART_SecondAuxiliaryButton.Visibility = Visibility.Visible;
                        }
                    }
                    break;
            }

            md.AffirmativeButtonText = md.DialogSettings.AffirmativeButtonText;
            md.NegativeButtonText = md.DialogSettings.NegativeButtonText;
            md.FirstAuxiliaryButtonText = md.DialogSettings.FirstAuxiliaryButtonText;
            md.SecondAuxiliaryButtonText = md.DialogSettings.SecondAuxiliaryButtonText;

            switch (md.DialogSettings.ColorScheme)
            {
                case TMPDialogColorScheme.Accented:
                    md.PART_NegativeButton.Style = md.FindResource("AccentedDialogHighlightedButton") as Style;
                    md.PART_FirstAuxiliaryButton.Style = md.FindResource("AccentedDialogHighlightedButton") as Style;
                    md.PART_SecondAuxiliaryButton.Style = md.FindResource("AccentedDialogHighlightedButton") as Style;
                    break;
            }
        }

        private void Dialog_Loaded(object sender, RoutedEventArgs e)
        {
            SetButtonState(this);
            SetDefaultButton();
        }

        public MessageDialogStyle ButtonStyle
        {
            get => (MessageDialogStyle)GetValue(ButtonStyleProperty);
            set => SetValue(ButtonStyleProperty, value);
        }

        public string Message
        {
            get => (string)GetValue(MessageProperty);
            set => SetValue(MessageProperty, value);
        }

        public string AffirmativeButtonText
        {
            get => (string)GetValue(AffirmativeButtonTextProperty);
            set => SetValue(AffirmativeButtonTextProperty, value);
        }

        public string NegativeButtonText
        {
            get => (string)GetValue(NegativeButtonTextProperty);
            set => SetValue(NegativeButtonTextProperty, value);
        }

        public string FirstAuxiliaryButtonText
        {
            get => (string)GetValue(FirstAuxiliaryButtonTextProperty);
            set => SetValue(FirstAuxiliaryButtonTextProperty, value);
        }

        public string SecondAuxiliaryButtonText
        {
            get => (string)GetValue(SecondAuxiliaryButtonTextProperty);
            set => SetValue(SecondAuxiliaryButtonTextProperty, value);
        }
    }

    /// <summary>
    /// Перечисление, представляющее результат Message Dialog
    /// </summary>
    public enum MessageDialogResult
    {
        Negative = 0,
        Affirmative = 1,
        FirstAuxiliary,
        SecondAuxiliary,
    }

    /// <summary>
    /// An enum representing the different button states for a Message Dialog.
    /// </summary>
    public enum MessageDialogStyle
    {
        /// <summary>
        /// Только "OK"
        /// </summary>
        Affirmative = 0,

        /// <summary>
        /// "OK" и "Отмена"
        /// </summary>
        AffirmativeAndNegative = 1,

        AffirmativeAndNegativeAndSingleAuxiliary = 2,
        AffirmativeAndNegativeAndDoubleAuxiliary = 3
    }
}