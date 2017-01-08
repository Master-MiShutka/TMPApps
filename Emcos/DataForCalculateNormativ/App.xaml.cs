using System;
using System.Windows;
using System.Windows.Threading;

namespace TMP.Work.Emcos.DataForCalculateNormativ
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            TMP.Work.Emcos.DataForCalculateNormativ.Properties.Settings.Default.Save();
        }

        public static readonly string Title = "Emcos";

        internal static MessageBoxResult ShowError(string message)
        {
            System.Media.SystemSounds.Exclamation.Play();
            return MessageBox.Show(message, Title, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        internal static MessageBoxResult ShowWarning(string message)
        {
            System.Media.SystemSounds.Hand.Play();
            return MessageBox.Show(message, Title, MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        internal static MessageBoxResult ShowInfo(string message)
        {
            System.Media.SystemSounds.Asterisk.Play();
            return MessageBox.Show(message, Title, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        internal static MessageBoxResult ShowQuestion(string message)
        {
            System.Media.SystemSounds.Question.Play();
            return MessageBox.Show(message, Title, MessageBoxButton.OK, MessageBoxImage.Question);
        }
        internal static string GetExceptionDetails(Exception exp)
        {
            string message = string.Empty;
            try
            {
                // Write Message tree of inner exception into textual representation
                message = exp.Message;

                Exception innerEx = exp.InnerException;

                for (int i = 0; innerEx != null; i++, innerEx = innerEx.InnerException)
                {
                    string spaces = string.Empty;

                    for (int j = 0; j < i; j++)
                        spaces += "  ";

                    message += "\n" + spaces + "└─>" + innerEx.Message;
                }

                // Write complete stack trace info into details section
                //message += "\nStack trace:\n" + exp.ToString();
            }
            catch
            {
            }

            return message;

        }

        internal static string ObjectToBase64String<T>(T value)
        {
            byte[] buffer;
            using (System.IO.MemoryStream msCompressed = new System.IO.MemoryStream())
            {
                using (System.IO.Compression.GZipStream gz = new System.IO.Compression.GZipStream(msCompressed, System.IO.Compression.CompressionMode.Compress))
                {
                    System.Runtime.Serialization.Formatters.Binary.BinaryFormatter bf = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                    bf.Serialize(gz, value);
                }
                buffer = msCompressed.ToArray();
            }
            return Convert.ToBase64String(buffer);
        }
        internal static T Base64StringToObject<T>(string value)
        {
            T result;
            byte[] buffer = Convert.FromBase64String(value);
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream(buffer))
            using (System.IO.Compression.GZipStream gz = new System.IO.Compression.GZipStream(ms, System.IO.Compression.CompressionMode.Decompress))
            {
                result = (T)new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter().Deserialize(gz);
            }
            return result;
        }

        internal static Dispatcher UIDispatcher;
        internal static void UIAction(Action action)
        {
            if (UIDispatcher.CheckAccess())
                action();
            else
                UIDispatcher.BeginInvoke(((Action)(() => { action(); })));
        }
    }
}
