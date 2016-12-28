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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TMP.Work.Emcos.DataForCalculateNormativ
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnGet_Click(object sender, RoutedEventArgs e)
        {
            ;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            ;
        }

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            // Id 52 Белэнерго Code 1
            // Id 53 РУП Гродноэнерго Code 14
            // Id 60 Ошмянские ЭС Code 143

            decimal nodeId = 60;
            GetElementsAsync(nodeId)
                .ContinueWith(t =>
                {
                    Dispatcher.Invoke((Action)(() => departamentsList.ItemsSource = t.Result));
                });
        }

        private void departamentsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            substationsList.Items.Clear();

            if (e.AddedItems == null && e.AddedItems.Count == 0)
                return;
            ListPoint point = e.AddedItems[0] as ListPoint;
            if (point == null)
                return;
            decimal nodeId = point.Id;
            GetElementsAsync(nodeId)
                .ContinueWith(t =>
                {
                    Dispatcher.Invoke((Action)(() => substationsList.ItemsSource = t.Result));
                });
        }

        private void substationsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            voltagesList.Items.Clear();

            if (e.AddedItems == null && e.AddedItems.Count == 0)
                return;
            ListPoint point = e.AddedItems[0] as ListPoint;
            if (point == null)
                return;
            decimal nodeId = point.Id;
            GetElementsAsync(nodeId)
                .ContinueWith(t =>
                {
                    Dispatcher.Invoke((Action)(() => voltagesList.ItemsSource = t.Result));
                });
        }

        private Task<IList<ListPoint>> GetElementsAsync(decimal id)
        {
            Cursor = Cursors.Wait;
            Task<string> task = EmcosSiteWrapper.Instance.GetAPointsAsync(id.ToString());
            task.ContinueWith(t =>
                {
                    Dispatcher.Invoke((Action)(() => App.ShowWarning(EmcosSiteWrapper.Instance.ErrorMessage)));
                }, TaskContinuationOptions.OnlyOnFaulted);
            return task.ContinueWith<IList<ListPoint>>(t =>
                {
                    string data = t.Result;
                    if (String.IsNullOrWhiteSpace(data))
                    {
                        Dispatcher.Invoke((Action)(() => App.ShowWarning(EmcosSiteWrapper.Instance.ErrorMessage)));
                        Cursor = Cursors.Arrow;
                        return new List<ListPoint>();
                    }
                    var records = Utils.ParseRecords(data);
                    if (records == null)
                        return new List<ListPoint>();

                    var list = new List<Model.IEmcosElement>();
                    foreach (var nvc in records)
                    {
                        Emcos.Model.IEmcosElement element;
                        if (nvc.Get("Type") == "POINT")
                            element = new Model.EmcosPointElement();
                        else
                            element = new Model.EmcosGrElement();

                        for (int i = 0; i < nvc.Count; i++)
                        {
                            #region Разбор полей
                            int intValue = 0;
                            switch (nvc.GetKey(i))
                            {
                                case "GR_ID":
                                    int.TryParse(nvc[i], out intValue);
                                    element.Id = intValue;
                                    break;
                                case "POINT_ID":
                                    int.TryParse(nvc[i], out intValue);
                                    element.Id = intValue;
                                    break;
                                case "GR_NAME":
                                    element.Name = nvc[i];
                                    break;
                                case "POINT_NAME":
                                    element.Name = nvc[i];
                                    break;
                                case "TYPECODE":
                                    element.TypeCode = nvc[i];
                                    break;
                            }
                            #endregion
                        }
                        list.Add(element);
                    }
                    IList<ListPoint> points = list.Select(i => new ListPoint { Id = i.Id, Name = i.Name, IsGroup = i.TypeCode == "GROUP" }).ToList();
                    Cursor = Cursors.Arrow;
                    return points;
                }, TaskContinuationOptions.NotOnFaulted);            
        }

    }

    public class ListPoint
    {
        public decimal Id { get; set; }
        public string Name { get; set; }
        public bool IsGroup { get; set; }
        public bool Checked { get; set; }
    }
}
