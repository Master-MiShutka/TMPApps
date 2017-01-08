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
using System.Threading.Tasks;

namespace Test
{
    using TMP.Wpf.Common.Controls.TableView;
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Task _task;
        public List<TestClass> Items { get; set; }
        public MainWindow()
        {
            InitializeComponent();

            Items = new List<TestClass>();
            Random rnd = new Random();
            for (int i = 0; i < 100000; i++)
                Items.Add(new TestClass()
                {
                    Date = DateTime.Now.AddDays(rnd.NextDouble()*10d),
                    VvodaIn = rnd.NextDouble()* rnd.NextDouble() + rnd.NextDouble()*100d
                });

            Test.EmcosServiceReference.ServiceSoapClient client = new EmcosServiceReference.ServiceSoapClient();
            var table = client.GetSTPLData("PSDTU_SERVER", "PPOINT_CODE_ID", "PML_ID", "01.12.2016", "31.12.2016");

            DataContext = this;
        }
        private void Create_tableColumns()
        {
            table.Columns.Clear();

            DataTemplate datecell = this.TryFindResource("dateTableViewCellTemplate") as DataTemplate;
            if (datecell == null)
                throw new NullReferenceException("Не найден шаблон: dateTableViewCellTemplate");
            DataTemplate textCell = this.TryFindResource("textTableViewCellTemplate") as DataTemplate;
            if (textCell == null)
                throw new NullReferenceException("Не найден шаблон: textTableViewCellTemplate");
            DataTemplate columnTitleTemplate = this.TryFindResource("tableViewColumnHeaderDataTemplate") as DataTemplate;
            if (columnTitleTemplate == null)
                throw new NullReferenceException("Не найден шаблон: tableViewColumnHeaderDataTemplate");

            System.Collections.ObjectModel.ObservableCollection<TableViewColumn> columns = new System.Collections.ObjectModel.ObservableCollection<TableViewColumn>();

            columns.Add(new TableViewColumn()
            {
                Title = "Data",
                ContextBinding = new Binding("Date"),
                CellTemplate = datecell
            });
            columns.Add(new TableViewColumn()
            {
                Title = "Header 1",
                ContextBinding = new Binding("VvodaIn"),
                Width = 110,
                CellTemplate = textCell,
                UseHistogramm = true,
                ShowHistogramm = true
            });
            foreach (TableViewColumn column in columns)
                column.TitleTemplate = columnTitleTemplate;
            table.Columns = columns;
        }

        public class TestClass
        {
            public DateTime Date { get; set; }
            public double VvodaIn { get; set; }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Cursor = Cursors.Wait;
            table.BeginInit();
            _task = Task.Factory.StartNew(() =>
            {
                //System.Threading.Thread.Sleep(5000);
                table.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background,
                    new Action(delegate ()
                    {
                        Create_tableColumns();
                        table.EndInit();
                        Cursor = Cursors.Arrow;
                    }));
            });

            _task.ContinueWith((t) =>
            {
                MessageBox.Show("Произошла ошибка.\n" + t.Exception.Message, this.Title, MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }, TaskContinuationOptions.OnlyOnFaulted);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("ggtfecgfe", Title, MessageBoxButton.YesNoCancel, MessageBoxImage.Asterisk);
        }
    }
}
