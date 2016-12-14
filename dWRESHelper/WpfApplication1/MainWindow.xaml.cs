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
using System.ComponentModel;

using QuickGraph;

namespace WpfApplication1
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private TestGraph _graphToVisualize;

        public TestGraph GraphToVisualize
        {
            get { return _graphToVisualize; }
            set { _graphToVisualize = value; NotifyPropertyChanged("GraphToVisualize"); }
        }

        public MainWindow()
        {
            CreateGraphToVisualize();

            InitializeComponent();
        }

        private void CreateGraphToVisualize()
        {
            var g = new TestGraph();
           
            Random rnd = new Random(DateTime.Now.Millisecond);

            int max_vertex = rnd.Next(5, 30);
            TestVertex[] vertices = new TestVertex[max_vertex];
            for (int i = 0; i < max_vertex; i++)
            {
                vertices[i] = new TestVertex(i, rnd.Next().ToString());
                g.AddVertex(vertices[i]);
            }

            int max_edge = rnd.Next(2, 10);
            for (int i = 0; i < max_vertex; i++)
            {
                int m = max_edge = rnd.Next(1, max_edge);
                for (int j = 0; j < m; j++)
                {
                    int n = 0;
                    while (n == 0 || n == i)
                        n = rnd.Next(0, max_vertex - 1);

                    g.AddEdge(rnd.Next(), vertices[i], vertices[n]);
                }
            }

            GraphToVisualize = g;
        }

        #region INotifyPropertyChanged Implementation

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        #endregion INotifyPropertyChanged Implementation

        private void ButtonRelayout_Click(object sender, RoutedEventArgs e)
        {
            graphLayout.Relayout();
        }

        private void ButtonGenerate_Click(object sender, RoutedEventArgs e)
        {
            CreateGraphToVisualize();
        }

        private void root_Loaded(object sender, RoutedEventArgs e)
        {
            MakeMenu();

            int count = this.Themes.Count;

            foreach (var t in this.Themes)
            {
                Button b = new Button()
                {
                    Content = t.ShortName
                };
                b.Click += b_Click;
                b.Tag = t;

                themesListPanel.Children.Add(b);
            }
        }

        private void b_Click(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            if (b == null) return;
            SelectedTheme = b.Tag as Theme;
        }
    }
}