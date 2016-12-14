using System;
using System.Collections.Generic;

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
using System.Xml.Serialization;
using System.IO;
using System.Threading.Tasks;


using TMP.ExcelXml;
using TMP.ARMTES.Model;

namespace TMP.ARMTES.Editor
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = MainModel.Instance;

            /*CollectionView myView;
            myView = (CollectionView)CollectionViewSource.GetDefaultView(collectorList.ItemsSource);
            if (myView.CanGroup == true)
            {
                PropertyGroupDescription groupDescription
                    = new PropertyGroupDescription("Departament");
                myView.GroupDescriptions.Add(groupDescription);
            }*/
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (System.IO.File.Exists("sdsp.list"))
            {
                DeSerialize();
            }
            else
                XMLExcelParser.Parse("data.xml").ContinueWith(x => 
                {
                    RegistrySDSP sdsp = x.Result;
                    if (sdsp == null) return;
                    MainModel.Instance.SDSP = sdsp;
                });
            if (MainModel.Instance.SDSP != null)
                departamentsList.SelectedIndex = 0;
        }

        private async void Serialize()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(RegistrySDSP));
            using (TextWriter writer = new StreamWriter("sdsp.list"))
            {
                await Task.Factory.StartNew(() => serializer.Serialize(writer, MainModel.Instance.SDSP));
            } 
        }

        private async void DeSerialize()
        {
            XmlSerializer deserializer = new XmlSerializer(typeof(RegistrySDSP));
            using (TextReader reader = new StreamReader("sdsp.list"))
            {
                await Task.Factory.StartNew(() =>
                    {
                        object obj = deserializer.Deserialize(reader);
                        MainModel.Instance.SDSP = (RegistrySDSP)obj;
                    });
            }
        }

        private void SerializeButton_Click(object sender, RoutedEventArgs e)
        {
            Serialize();
        }

        private void DeSerializeButton_Click(object sender, RoutedEventArgs e)
        {
            DeSerialize();
        }

        CollectionView myView;
        private void AddGrouping(object sender, RoutedEventArgs e)
        {
            myView = (CollectionView)CollectionViewSource.GetDefaultView(collectorList.ItemsSource);
            if (myView.CanGroup == true)
            {
                PropertyGroupDescription groupDescription
                    = new PropertyGroupDescription("Departament");
                myView.GroupDescriptions.Add(groupDescription);
            }
            else
                return;
        }

        private void RemoveGrouping(object sender, RoutedEventArgs e)
        {
            myView = (CollectionView)CollectionViewSource.GetDefaultView(collectorList.ItemsSource);
            myView.GroupDescriptions.Clear();
        }

        private void departamentsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string departament = e.AddedItems[0].ToString();

            MainModel.Instance.SDSP.FilterValue = departament;
            MainModel.Instance.SDSP.FilterProperty = "Departament";
        }

        private void collectorList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            AddEditCollectorWindow editWindow;

            RegistryCollector collector = collectorList.SelectedValue as RegistryCollector;
            if (collector == null)
                editWindow = new AddEditCollectorWindow();
            else
                editWindow = new AddEditCollectorWindow(collector);
            
            editWindow.Owner = this;
            try
            {
                bool? showing = editWindow.ShowDialog();

                if (showing.HasValue && showing.Value)
                {
                    ;
                }
            }
            finally
            {
                editWindow = null;
            }

        }

        private void collectorList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ;
        }
    }
}
