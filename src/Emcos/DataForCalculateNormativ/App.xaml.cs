using System.Windows;

namespace TMP.Work.Emcos.DataForCalculateNormativ
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : TMPApplication.TMPApp
    {              
        public App()
        {
            Exit += (s, e) =>
            {
                Log("Сохранение параметров");            
                DataForCalculateNormativ.Properties.Settings.Default.Save();
            };

            Title = "Получение режимных данных из Emcos";
        }

        public static EmcosTestWebService.ServiceSoapClient EmcosWebServiceClient { get; private set; }

        public static void InitServiceClient(string address)
        {
            System.ServiceModel.Channels.Binding binding = new System.ServiceModel.BasicHttpBinding()
            {
                MaxBufferSize = 10240000,
                MaxReceivedMessageSize = 10240000,
                Name = "ServiceSoap"
            };
            EmcosWebServiceClient = new EmcosTestWebService.ServiceSoapClient(binding,
                new System.ServiceModel.EndpointAddress(address));

            /*FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement), 
                new FrameworkPropertyMetadata(System.Windows.Markup.XmlLanguage.GetLanguage(System.Globalization.CultureInfo.CurrentCulture.IetfLanguageTag)));*/
        }
    }
}
