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

namespace TMP.Wpf.CommonControls.Viewers
{
    /// <summary>
    /// Interaction logic for TextJsonViewer.xaml
    /// </summary>
    public partial class TextJsonViewer : UserControl, IStringBasedViewer
    {
        public TextJsonViewer(string content)
        {          
            InitializeComponent();
            textbox.Text = content;
        }

        public void SaveContent()
        {
            try
            {
                Microsoft.Win32.SaveFileDialog sfd = new Microsoft.Win32.SaveFileDialog();
                sfd.DefaultExt = ".json";
                sfd.Filter = "Файлы JSON (*.json)|*.json";
                sfd.FilterIndex = 0;
                sfd.Title = "Сохранение данных";
                if (sfd.ShowDialog() == true)
                    System.IO.File.WriteAllText(sfd.FileName, textbox.Text, Encoding.UTF8);
            }
            catch (System.IO.IOException ex)
            {
                MessageBox.Show("Произошла ошибка при сохранении.\n" + ex.Message, "Сохранение", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveContent(object sender, RoutedEventArgs e)
        {
            SaveContent();
        }
    }
}
