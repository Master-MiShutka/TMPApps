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
using System.Windows.Threading;

namespace TestApp
{
    /// <summary>
    /// Interaction logic for TestWindow2.xaml
    /// </summary>
    public partial class TestWindow2 : Window
    {
        public TestWindow2()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            (this.DataContext as MatrixModel).Go();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.DataContext = new MatrixModel();
        }

        private void Set_aero_normalcolor(object sender, RoutedEventArgs e)
        {
            LoadTheme(themesList[0]);
        }

        private void Set_aero2_normalcolor(object sender, RoutedEventArgs e)
        {
            LoadTheme(themesList[1]);
        }

        private void Set_classic(object sender, RoutedEventArgs e)
        {
            LoadTheme(themesList[2]);
        }
        private void Set_luna_normalcolor(object sender, RoutedEventArgs e)
        {
            LoadTheme(themesList[3]);
        }

        private void Set_luna_homestead(object sender, RoutedEventArgs e)
        {
            LoadTheme(themesList[4]);
        }

        private void Set_luna_metallic(object sender, RoutedEventArgs e)
        {
            LoadTheme(themesList[5]);
        }

        private void Set_royale_mnormalcolor(object sender, RoutedEventArgs e)
        {
            LoadTheme(themesList[6]);
        }

        private void Set_zune_normalcolor(object sender, RoutedEventArgs e)
        {
            ThemeHelper.SetTheme("zune", "normalcolor");
        }

        private string[] themesList = new string[7]{
                        "/PresentationFramework.Aero;component/themes/aero.normalcolor.xaml",
                        "/PresentationFramework.Aero2;component/themes/aero2.normalcolor.xaml",
                        "/PresentationFramework.classic;component/themes/classic.xaml",
                        "/PresentationFramework.Luna;component/themes/luna.normalcolor.xaml",
                        "/PresentationFramework.Luna;component/themes/luna.homestead.xaml",
                        "/PresentationFramework.Luna;component/themes/luna.metallic.xaml",
                        "/PresentationFramework.Royale;component/themes/royale.normalcolor.xaml"};

        private void LoadTheme(string themePath)
        {
            List<Uri> dictionaries = null;
            if (Application.Current.Resources.MergedDictionaries != null)
            {
                dictionaries = Application.Current.Resources.MergedDictionaries.Where(d => d.Source.OriginalString.StartsWith(@"/PresentationFramework") == false).Select(d => d.Source).ToList();
                // очищаем перед загрузкой темы
                App.Current.Resources.MergedDictionaries.Clear();
            }
            // загружаем необходимые для работы ресурсы
            // загружаем тему
            try
            {
                Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = new Uri(themePath, UriKind.RelativeOrAbsolute) });
                if (dictionaries != null)
                    dictionaries.ForEach((uri) => Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = uri }));
            }
            catch (Exception)
            {
                MessageBox.Show("Не удалось применить тему.", "MY", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
