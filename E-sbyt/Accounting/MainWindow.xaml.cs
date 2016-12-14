using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using System.Xml.Linq;

namespace TMP.Work.ESbyt.Accounting
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string SERVERURL = "http://localhost/esbyt/hs/web/";//10.182.5.13
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            /*Common.RequestState rs;
            Common.BufferedAsyncNetClient banc = new Common.BufferedAsyncNetClient(SERVERURL);
            string result = banc.Get("", SERVERURL + "getpoint?id=14310000001000002&format=json&debug=1");
            rs = banc.State;

            if (rs != null && rs.Response != null)
            {
                if (rs.Response.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    MessageBox.Show("Ошибка. Код ответа " + rs.Response.StatusCode.ToString() + "\n" + rs.Response.StatusDescription, 
                        Title, MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                switch (rs.Response.ContentType)
                {
                    case "application/json;charset=utf-8":
                        Model.PointInfo pi = Common.NetHelper<Model.PointInfo>.Deserialize(result);

                        break;
                }
            }*/
            XDocument doc = XDocument.Load(@"Data\договоры.xml");

            var dogovors = doc.Root.Elements("row").Select(el => new Dog()
            {
                филиал = el.Attribute("филиал").Value,
                ПометкаУдаления = el.Attribute("ПометкаУдаления").Value,
                Код = el.Attribute("Код").Value,
                Наименование = el.Attribute("Наименование").Value,
                номер = el.Attribute("номер").Value,
                АбонентНаименование = el.Attribute("АбонентНаименование").Value,
                ГруппаПотребленияНаименование = el.Attribute("ГруппаПотребленияНаименование").Value,
                РЭСНаименование = el.Attribute("РЭСНаименование").Value,
                Субабонент = el.Attribute("Субабонент").Value
            });
            dg1.ItemsSource = dogovors;
        }
    }

    public class Dog
    {
        public string филиал { get; set; }
        public string ПометкаУдаления { get; set; }
        public string Код { get; set; }
        public string Наименование { get; set; }
        public string номер { get; set; }
        public string АбонентНаименование { get; set; }
        public string ГруппаПотребленияНаименование { get; set; }
        public string РЭСНаименование { get; set; }
        public string Субабонент { get; set; }
    }
}
