using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

using Windows.UI.Notifications;

namespace WpfApp_ToastTest
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static string _tostImageTempFileName;
        private string _appName = "WpfApp_ToastTest";
        private const string APP_ID = "WpfApp_ToastTest";
        private const string Title = "Testing Windows Toast";

        public App()
        {
            this.RegisterAppForNotificationSupport();
            ShellHelpers.NotificationActivator.Initialize();
            _tostImageTempFileName = System.IO.Path.GetTempFileName();
            using (System.IO.FileStream file = new System.IO.FileStream(_tostImageTempFileName, System.IO.FileMode.Create, System.IO.FileAccess.Write))
                WpfApp_ToastTest.Properties.Resources.logo.Save(file);
        }
        ~App()
        {
            try
            {
                System.IO.File.Delete(_tostImageTempFileName);
            }
            catch { }
        }

        public static void ToastActivated()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                App.Current.MainWindow.Activate();
                if (App.Current.MainWindow.WindowState == WindowState.Minimized)
                    App.Current.MainWindow.WindowState = WindowState.Normal;
            });
        }

        #region Windows 10 Notification support

        // In order to display toasts, a desktop application must have a shortcut on the Start menu.
        // Also, an AppUserModelID must be set on that shortcut.
        //
        // For the app to be activated from Action Center, it needs to register a COM server with the OS
        // and register the CLSID of that COM server on the shortcut.
        //
        // The shortcut should be created as part of the installer. The following code shows how to create
        // a shortcut and assign the AppUserModelID and ToastActivatorCLSID properties using Windows APIs.
        //
        // Included in this project is a wxs file that be used with the WiX toolkit
        // to make an installer that creates the necessary shortcut. One or the other should be used.
        //
        // This sample doesn't clean up the shortcut or COM registration.

        private void RegisterAppForNotificationSupport()
        {
            String shortcutPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Microsoft\\Windows\\Start Menu\\Programs\\" + _appName + ".lnk";
            if (!System.IO.File.Exists(shortcutPath))
            {
                String exePath = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
                this.InstallShortcut(shortcutPath, exePath);
                this.RegisterComServer(exePath);
            }
        }

        private void InstallShortcut(String shortcutPath, String exePath)
        {
            ShellHelpers.IShellLinkW newShortcut = (ShellHelpers.IShellLinkW)new ShellHelpers.CShellLink();

            // Create a shortcut to the exe
            newShortcut.SetPath(exePath);

            // Open the shortcut property store, set the AppUserModelId property
            ShellHelpers.IPropertyStore newShortcutProperties = (ShellHelpers.IPropertyStore)newShortcut;

            ShellHelpers.PropVariantHelper varAppId = new ShellHelpers.PropVariantHelper();
            varAppId.SetValue(APP_ID);
            newShortcutProperties.SetValue(ShellHelpers.PROPERTYKEY.AppUserModel_ID, varAppId.Propvariant);

            ShellHelpers.PropVariantHelper varToastId = new ShellHelpers.PropVariantHelper()
            {
                VarType = System.Runtime.InteropServices.VarEnum.VT_CLSID
            };
            varToastId.SetValue(typeof(ShellHelpers.NotificationActivator).GUID);

            newShortcutProperties.SetValue(ShellHelpers.PROPERTYKEY.AppUserModel_ToastActivatorCLSID, varToastId.Propvariant);

            // Commit the shortcut to disk
            ShellHelpers.IPersistFile newShortcutSave = (ShellHelpers.IPersistFile)newShortcut;

            newShortcutSave.Save(shortcutPath, true);
        }

        private void RegisterComServer(String exePath)
        {
            // We register the app process itself to start up when the notification is activated, but
            // other options like launching a background process instead that then decides to launch
            // the UI as needed.
            string regString = String.Format("SOFTWARE\\Classes\\CLSID\\{{{0}}}\\LocalServer32", typeof(ShellHelpers.NotificationActivator).GUID);
            try
            {
                var key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(regString);
                key.SetValue(null, exePath);
            }
            catch
            {
            }
        }

        public static void ShowToast(string message)
        {
            // Get a toast XML template
            Windows.Data.Xml.Dom.XmlDocument toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastImageAndText04);

            // Fill in the text elements
            Windows.Data.Xml.Dom.XmlNodeList stringElements = toastXml.GetElementsByTagName("text");
            ((IReadOnlyList<Windows.Data.Xml.Dom.IXmlNode>)stringElements)[0].AppendChild(toastXml.CreateTextNode(App.Title));
            ((IReadOnlyList<Windows.Data.Xml.Dom.IXmlNode>)stringElements)[1].AppendChild(toastXml.CreateTextNode(message));

            App.ToastSetAttribute(ref toastXml, "toast", "duration", "short");

            // Specify the absolute path to an image as a URI
            String imagePath = new System.Uri(System.IO.Path.GetFullPath(_tostImageTempFileName)).AbsoluteUri;
            Windows.Data.Xml.Dom.XmlNodeList imageElements = toastXml.GetElementsByTagName("image");
            imageElements[0].Attributes.GetNamedItem("src").NodeValue = imagePath;

            double time = 10d;

            // Create the toast and attach event listeners
            ToastNotification toast = new ToastNotification(toastXml);
            if (time > 0)
            {
                toast.ExpirationTime = new DateTimeOffset?(System.DateTime.Now.AddSeconds((double)time));
            }
            toast.Activated += ToastActivated;
            toast.Dismissed += ToastDismissed;
            toast.Failed += ToastFailed;

            // Show the toast. Be sure to specify the AppUserModelId on your application's shortcut!
            ToastNotificationManager.CreateToastNotifier(APP_ID).Show(toast);
        }

        private static void ToastSetAttribute(ref Windows.Data.Xml.Dom.XmlDocument Toast, string Element, string Name, string Value)
        {
            ((Windows.Data.Xml.Dom.XmlElement)((IReadOnlyList<Windows.Data.Xml.Dom.IXmlNode>)Toast.GetElementsByTagName(Element))[0]).SetAttribute(Name, Value);
        }

        public static void ToastActivated(ToastNotification sender, object e)
        {
            ToastActivatedEventArgs args = (ToastActivatedEventArgs)e;
            if (args.Arguments == "Open")
            {
                ;
            }

            ToastActivated();
        }

        private static void ToastDismissed(ToastNotification sender, ToastDismissedEventArgs e)
        {
            String outputText = "";
            switch (e.Reason)
            {
                case ToastDismissalReason.ApplicationHidden:
                    outputText = "The app hid the toast using ToastNotifier.Hide";
                    break;
                case ToastDismissalReason.UserCanceled:
                    outputText = "The user dismissed the toast";
                    break;
                case ToastDismissalReason.TimedOut:
                    outputText = "The toast has timed out";
                    break;
                default:
                    outputText = e.Reason.ToString();
                    break;
            }

            Application.Current.Dispatcher.Invoke(() =>
            {
                MessageBox.Show(outputText);
            });
        }

        private static void ToastFailed(ToastNotification sender, ToastFailedEventArgs e)
        {
            MessageBox.Show("The toast encountered an error.", Title, MessageBoxButton.OK, MessageBoxImage.Error);
        }
        private void ToastAddButton(ref Windows.Data.Xml.Dom.XmlDocument Toast, string Content, string Arguments)
        {
            ((IReadOnlyList<Windows.Data.Xml.Dom.IXmlNode>)Toast.GetElementsByTagName("toast"))[0].AppendChild(Toast.CreateElement("actions"));
            IReadOnlyList<Windows.Data.Xml.Dom.IXmlNode> actions = Toast.GetElementsByTagName("actions");
            Windows.Data.Xml.Dom.XmlElement xmlElement = Toast.CreateElement("action");
            xmlElement.SetAttribute("content", Content);
            xmlElement.SetAttribute("arguments", Arguments);
            actions[0].AppendChild(xmlElement);
        }

        #endregion
    }
}
