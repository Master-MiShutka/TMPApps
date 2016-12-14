using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace TMP.Work.Emcos
{
    /// <summary>
    /// Interaction logic for MessageBox.xaml
    /// </summary>
    public partial class MessageBox : Window
    {
        public MessageBox(string message)
        {
            InitializeComponent();
            DataContext = this;
            SetValue(MessageProperty, message);
        }

        private const string DefaultMessage = "???";

        public static readonly DependencyProperty MessageProperty = DependencyProperty.Register("Message", typeof(string), typeof(MessageBox), new PropertyMetadata(String.Empty));

        [Bindable(true)]
        [DefaultValue(DefaultMessage)]
        [Category("Behavior")]
        public string Message
        {
            get { return (string)GetValue(MessageProperty); }
            set { SetValue(MessageProperty, value); }
        }
        public static readonly DependencyProperty MessageBoxButtonProperty = DependencyProperty.Register("MessageBoxButton", typeof(MessageBoxButton), typeof(MessageBox), new PropertyMetadata(MessageBoxButton.OK));

        [Bindable(true)]
        [DefaultValue(MessageBoxButton.OK)]
        [Category("Behavior")]
        public MessageBoxButton MessageBoxButton
        {
            get { return (MessageBoxButton)GetValue(MessageBoxButtonProperty); }
            set { SetValue(MessageBoxButtonProperty, value); }
        }
        public static readonly DependencyProperty MessageBoxImageProperty = DependencyProperty.Register("MessageBoxImage", typeof(MessageBoxImage), typeof(MessageBox), new PropertyMetadata(MessageBoxImage.None));

        [Bindable(true)]
        [DefaultValue(MessageBoxImage.None)]
        [Category("Behavior")]
        public MessageBoxImage MessageBoxImage
        {
            get { return (MessageBoxImage)GetValue(MessageBoxImageProperty); }
            set { SetValue(MessageBoxImageProperty, value); }
        }
        public static readonly DependencyProperty IconKindProperty = DependencyProperty.Register("IconKind", typeof(TMP.Wpf.Common.Icons.PackIconModernKind), typeof(MessageBox), new PropertyMetadata(TMP.Wpf.Common.Icons.PackIconModernKind.InformationCircle));

        [Bindable(true)]
        [DefaultValue(TMP.Wpf.Common.Icons.PackIconModernKind.InformationCircle)]
        [Category("Behavior")]
        public TMP.Wpf.Common.Icons.PackIconModernKind IconKind
        {
            get { return (TMP.Wpf.Common.Icons.PackIconModernKind)GetValue(IconKindProperty); }
            set { SetValue(IconKindProperty, value); }
        }

        public static new MessageBoxResult DialogResult { get; set; }

        public static MessageBoxResult Show(string message)
        {
            System.Media.SystemSounds.Beep.Play();
            var msgbox = new MessageBox(message);
            msgbox.Title = "Сообщение";           

            if (msgbox.ShowDialog().HasValue)
                return DialogResult;
            else
                return MessageBoxResult.OK;
        }
        public static MessageBoxResult Show(string message, string title)
        {
            System.Media.SystemSounds.Beep.Play();
            var msgbox = new MessageBox(message);
            msgbox.Title = title;
            if (msgbox.ShowDialog().HasValue)
                return DialogResult;
            else
                return MessageBoxResult.OK;
        }
        public static MessageBoxResult Show(string message, string title, MessageBoxButton button)
        {
            System.Media.SystemSounds.Beep.Play();
            var msgbox = new MessageBox(message);
            msgbox.Title = title;

            if (button == MessageBoxButton.OKCancel)
            {
                msgbox.btn1.Visibility = Visibility.Visible;
                msgbox.btn1.Content = "OK";
                msgbox.btn1.Click += (s, e) =>
                {
                    DialogResult = MessageBoxResult.OK;
                    msgbox.Close();
                };
                msgbox.btn2.Visibility = Visibility.Visible;
                msgbox.btn2.Content = "Отменить";
                msgbox.btn2.Click += (s, e) =>
                {
                    DialogResult = MessageBoxResult.Cancel;
                    msgbox.Close();
                };
            }

            if (msgbox.ShowDialog().HasValue)
                return DialogResult;
            else
                return MessageBoxResult.OK;
        }
        public static MessageBoxResult Show(string message, string title, MessageBoxButton button, MessageBoxImage image)
        {
            var msgbox = new MessageBox(message);
            msgbox.Title = title;
            if (image == MessageBoxImage.None)
            {
                System.Media.SystemSounds.Beep.Play();
                msgbox.SetValue(IconKindProperty, TMP.Wpf.Common.Icons.PackIconModernKind.None);
            }

            if (image == MessageBoxImage.Question)
            {
                System.Media.SystemSounds.Question.Play();
                msgbox.SetValue(IconKindProperty, TMP.Wpf.Common.Icons.PackIconModernKind.Question);
                msgbox.Foreground = System.Windows.Media.Brushes.Green;
            }

            if (image == MessageBoxImage.Hand || image == MessageBoxImage.Information)
            {
                System.Media.SystemSounds.Hand.Play();
                msgbox.SetValue(IconKindProperty, TMP.Wpf.Common.Icons.PackIconModernKind.InformationCircle);
                msgbox.Foreground = System.Windows.Media.Brushes.Navy;
            }

            if (image == MessageBoxImage.Warning || image == MessageBoxImage.Asterisk)
            {
                System.Media.SystemSounds.Asterisk.Play();
                msgbox.SetValue(IconKindProperty, TMP.Wpf.Common.Icons.PackIconModernKind.WarningCircle);
                msgbox.Foreground = System.Windows.Media.Brushes.Yellow;
            }

            if (image == MessageBoxImage.Exclamation || image == MessageBoxImage.Error)
            {
                System.Media.SystemSounds.Exclamation.Play();
                msgbox.SetValue(IconKindProperty, TMP.Wpf.Common.Icons.PackIconModernKind.Alert);
                msgbox.Foreground = System.Windows.Media.Brushes.Red;
            }

            if (button == MessageBoxButton.OKCancel)
            {
                msgbox.btn1.Visibility = Visibility.Visible;
                msgbox.btn1.Content = "OK";
                msgbox.btn1.Click += (s, e) =>
                {
                    DialogResult = MessageBoxResult.OK;
                    msgbox.Close();
                };
                msgbox.btn2.Visibility = Visibility.Visible;
                msgbox.btn2.Content = "Отменить";
                msgbox.btn2.Click += (s, e) =>
                {
                    DialogResult = MessageBoxResult.Cancel;
                    msgbox.Close();
                };
                msgbox.btnClose.Visibility = Visibility.Collapsed;
            }
            if (button == MessageBoxButton.YesNo)
            {
                msgbox.btn2.Visibility = Visibility.Visible;
                msgbox.btn2.Content = "Да";
                msgbox.btn2.Click += (s, e) =>
                {
                    DialogResult = MessageBoxResult.Yes;
                    msgbox.Close();
                };
                msgbox.btn1.Visibility = Visibility.Visible;
                msgbox.btn1.Content = "Нет";
                msgbox.btn1.Click += (s, e) =>
                {
                    DialogResult = MessageBoxResult.No;
                    msgbox.Close();
                };
                msgbox.btnClose.Visibility = Visibility.Collapsed;
            }
            if (button == MessageBoxButton.YesNoCancel)
            {
                msgbox.btn3.Visibility = Visibility.Visible;
                msgbox.btn3.Content = "Да";
                msgbox.btn3.Click += (s, e) =>
                {
                    DialogResult = MessageBoxResult.Yes;
                    msgbox.Close();
                };
                msgbox.btn2.Visibility = Visibility.Visible;
                msgbox.btn2.Content = "Нет";
                msgbox.btn2.Click += (s, e) =>
                {
                    DialogResult = MessageBoxResult.No;
                    msgbox.Close();
                };
                msgbox.btn1.Visibility = Visibility.Visible;
                msgbox.btn1.Content = "Отменить";
                msgbox.btn1.Click += (s, e) =>
                {
                    DialogResult = MessageBoxResult.Cancel;
                    msgbox.Close();
                };
                msgbox.btnClose.Visibility = Visibility.Collapsed;
            }

            if (msgbox.ShowDialog().HasValue)
                return DialogResult;
            else
                return MessageBoxResult.OK;
        }
    }
}
