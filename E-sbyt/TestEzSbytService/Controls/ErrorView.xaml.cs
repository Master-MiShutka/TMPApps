using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TMP.Work.AmperM.TestApp.Controls
{
    /// <summary>
    /// Interaction logic for ErrorView.xaml
    /// </summary>
    public partial class ErrorView : UserControl
    {
        public ErrorView()
        {
            InitializeComponent();
            DataContext = this;
        }
        public ErrorView(String message) : this()
        {
            ErrorString = message;
        }
        public static readonly DependencyProperty ErrorStringProperty = DependencyProperty.Register(
            "ErrorString",
            typeof(string),
            typeof(ResultViewer),
            new FrameworkPropertyMetadata(null, OnPropertyChanged)
           );
        public string ErrorString
        {
            get { return (string)GetValue(ErrorStringProperty); }
            set
            {
                SetValue(ErrorStringProperty, value);
            }
        }
        private static void OnPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var rv = (ResultViewer)dependencyObject;
            switch (e.Property.Name)
            {
                case "ErrorString":
                    string value = e.NewValue as string;
                    if (string.IsNullOrEmpty(value))
                    {
                        rv.Visibility = Visibility.Hidden;
                    }
                    else
                    {
                        rv.Visibility = Visibility.Visible;
                    }
                    break;
            }
        }
        public void Show(string error)
        {
            ErrorString = error;
        }
    }
}
