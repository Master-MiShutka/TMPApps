﻿using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace TMP.Wpf.CommonControls.Dialogs
{
    public class LoginDialogSettings : TMPDialogSettings
    {
        private const string DefaultUsernameWatermark = "Имя пользователя...";
        private const string DefaultPasswordWatermark = "Пароль...";
        private const Visibility DefaultNegativeButtonVisibility = Visibility.Collapsed;

        public LoginDialogSettings()
        {
            UsernameWatermark = DefaultUsernameWatermark;
            PasswordWatermark = DefaultPasswordWatermark;
            NegativeButtonVisibility = DefaultNegativeButtonVisibility;
            AffirmativeButtonText = "Вход";
        }

        public string InitialUsername { get; set; }

        public string UsernameWatermark { get; set; }

        public string PasswordWatermark { get; set; }

        public Visibility NegativeButtonVisibility { get; set; }
    }

    public class LoginDialogData
    {
        public string Username { get; set; }

        public string Password { get; set; }
    }

    public partial class LoginDialog : BaseTMPDialog
    {
        internal LoginDialog(TMPWindow parentWindow)
            : this(parentWindow, null)
        {
        }

        internal LoginDialog(TMPWindow parentWindow, LoginDialogSettings settings)
            : base(parentWindow, settings)
        {
            InitializeComponent();
            Username = settings.InitialUsername;
            UsernameWatermark = settings.UsernameWatermark;
            PasswordWatermark = settings.PasswordWatermark;
            NegativeButtonButtonVisibility = settings.NegativeButtonVisibility;
        }

        internal Task<LoginDialogData> WaitForButtonPressAsync()
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                this.Focus();
                if (string.IsNullOrEmpty(PART_TextBox.Text))
                {
                    PART_TextBox.Focus();
                }
                else
                {
                    PART_TextBox2.Focus();
                }
            }));

            TaskCompletionSource<LoginDialogData> tcs = new TaskCompletionSource<LoginDialogData>();

            RoutedEventHandler negativeHandler = null;
            KeyEventHandler negativeKeyHandler = null;

            RoutedEventHandler affirmativeHandler = null;
            KeyEventHandler affirmativeKeyHandler = null;

            KeyEventHandler escapeKeyHandler = null;

            Action cleanUpHandlers = () =>
            {
                PART_TextBox.KeyDown -= affirmativeKeyHandler;
                PART_TextBox2.KeyDown -= affirmativeKeyHandler;

                this.KeyDown -= escapeKeyHandler;

                PART_NegativeButton.Click -= negativeHandler;
                PART_AffirmativeButton.Click -= affirmativeHandler;

                PART_NegativeButton.KeyDown -= negativeKeyHandler;
                PART_AffirmativeButton.KeyDown -= affirmativeKeyHandler;
            };

            escapeKeyHandler = (sender, e) =>
            {
                if (e.Key == Key.Escape)
                {
                    cleanUpHandlers();

                    tcs.TrySetResult(null);
                }
            };

            negativeKeyHandler = (sender, e) =>
            {
                if (e.Key == Key.Enter)
                {
                    cleanUpHandlers();

                    tcs.TrySetResult(null);
                }
            };

            affirmativeKeyHandler = (sender, e) =>
            {
                if (e.Key == Key.Enter)
                {
                    cleanUpHandlers();
                    tcs.TrySetResult(new LoginDialogData { Username = Username, Password = PART_TextBox2.Password });
                }
            };

            negativeHandler = (sender, e) =>
            {
                cleanUpHandlers();

                tcs.TrySetResult(null);

                e.Handled = true;
            };

            affirmativeHandler = (sender, e) =>
            {
                cleanUpHandlers();

                tcs.TrySetResult(new LoginDialogData { Username = Username, Password = PART_TextBox2.Password });

                e.Handled = true;
            };

            PART_NegativeButton.KeyDown += negativeKeyHandler;
            PART_AffirmativeButton.KeyDown += affirmativeKeyHandler;

            PART_TextBox.KeyDown += affirmativeKeyHandler;
            PART_TextBox2.KeyDown += affirmativeKeyHandler;

            this.KeyDown += escapeKeyHandler;

            PART_NegativeButton.Click += negativeHandler;
            PART_AffirmativeButton.Click += affirmativeHandler;

            return tcs.Task;
        }

        private void Dialog_Loaded(object sender, RoutedEventArgs e)
        {
            this.AffirmativeButtonText = this.DialogSettings.AffirmativeButtonText;
            this.NegativeButtonText = this.DialogSettings.NegativeButtonText;

            switch (this.DialogSettings.ColorScheme)
            {
                case TMPDialogColorScheme.Accented:
                    this.PART_NegativeButton.Style = this.FindResource("AccentedDialogHighlightedButton") as Style;
                    PART_TextBox.SetResourceReference(ForegroundProperty, "BlackColorBrush");
                    PART_TextBox2.SetResourceReference(ForegroundProperty, "BlackColorBrush");
                    break;
            }
        }

        public static readonly DependencyProperty MessageProperty = DependencyProperty.Register("Message", typeof(string), typeof(LoginDialog), new PropertyMetadata(default(string)));
        public static readonly DependencyProperty UsernameProperty = DependencyProperty.Register("Username", typeof(string), typeof(LoginDialog), new PropertyMetadata(default(string)));
        public static readonly DependencyProperty UsernameWatermarkProperty = DependencyProperty.Register("UsernameWatermark", typeof(string), typeof(LoginDialog), new PropertyMetadata(default(string)));
        public static readonly DependencyProperty PasswordProperty = DependencyProperty.Register("Password", typeof(string), typeof(LoginDialog), new PropertyMetadata(default(string)));
        public static readonly DependencyProperty PasswordWatermarkProperty = DependencyProperty.Register("PasswordWatermark", typeof(string), typeof(LoginDialog), new PropertyMetadata(default(string)));
        public static readonly DependencyProperty AffirmativeButtonTextProperty = DependencyProperty.Register("AffirmativeButtonText", typeof(string), typeof(LoginDialog), new PropertyMetadata("OK"));
        public static readonly DependencyProperty NegativeButtonTextProperty = DependencyProperty.Register("NegativeButtonText", typeof(string), typeof(LoginDialog), new PropertyMetadata("Cancel"));
        public static readonly DependencyProperty NegativeButtonButtonVisibilityProperty = DependencyProperty.Register("NegativeButtonButtonVisibility", typeof(Visibility), typeof(LoginDialog), new PropertyMetadata(Visibility.Collapsed));

        public string Message
        {
            get => (string)GetValue(MessageProperty);
            set => SetValue(MessageProperty, value);
        }

        public string Username
        {
            get => (string)GetValue(UsernameProperty);
            set => SetValue(UsernameProperty, value);
        }

        public string Password
        {
            get => (string)GetValue(PasswordProperty);
            set => SetValue(PasswordProperty, value);
        }

        public string UsernameWatermark
        {
            get => (string)GetValue(UsernameWatermarkProperty);
            set => SetValue(UsernameWatermarkProperty, value);
        }

        public string PasswordWatermark
        {
            get => (string)GetValue(PasswordWatermarkProperty);
            set => SetValue(PasswordWatermarkProperty, value);
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

        public Visibility NegativeButtonButtonVisibility
        {
            get => (Visibility)GetValue(NegativeButtonButtonVisibilityProperty);
            set => SetValue(NegativeButtonButtonVisibilityProperty, value);
        }
    }
}