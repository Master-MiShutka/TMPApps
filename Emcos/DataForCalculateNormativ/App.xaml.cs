using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;

namespace TMP.Work.Emcos.DataForCalculateNormativ
{
    using ServiceLocator;
    using MsgBox;
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static IMessageBoxService MessageBox = new MessageBoxService();
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            Emcos.EmcosSettings settings = new EmcosSettings(TMP.Work.Emcos.DataForCalculateNormativ.Properties.Settings.Default)
            {
                UserName = TMP.Work.Emcos.DataForCalculateNormativ.Properties.Settings.Default.UserName,
                Password = TMP.Work.Emcos.DataForCalculateNormativ.Properties.Settings.Default.Password,
                ServerAddress = TMP.Work.Emcos.DataForCalculateNormativ.Properties.Settings.Default.ServerAddress,
                ServiceName = TMP.Work.Emcos.DataForCalculateNormativ.Properties.Settings.Default.ServiceName,
                NetTimeOutInSeconds = TMP.Work.Emcos.DataForCalculateNormativ.Properties.Settings.Default.NetTimeOutInSeconds
            };
            Emcos.EmcosSiteWrapper.SetSettings(settings);
            ServiceContainer.Instance.AddService<IMessageBoxService>(MessageBox);
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            TMP.Work.Emcos.DataForCalculateNormativ.Properties.Settings.Default.Save();
        }

        public static readonly string Title = "Emcos";

        public static MsgBoxResult ShowInfo(string message)
        {
            return MessageBox.Show(message, Title, MsgBoxButtons.OK, MsgBoxImage.Information);
        }
        public static MsgBoxResult ShowWarning(string message)
        {
            return MessageBox.Show(message, Title, MsgBoxButtons.OK, MsgBoxImage.Warning);
        }
        public static MsgBox.MsgBoxResult ShowError(string message)
        {
            return MessageBox.Show(message, Title, MsgBoxButtons.OK, MsgBoxImage.Error);
        }
        public static MsgBox.MsgBoxResult ShowMessage(string message)
        {
            return MessageBox.Show(message, Title, MsgBoxButtons.OK);
        }
    }
}
