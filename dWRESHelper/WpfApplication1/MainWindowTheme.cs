using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Interop;

namespace WpfApplication1
{
    public partial class MainWindow : Window
    {
        #region Themes

        private Theme[] themesList = new Theme[9]
            {
                new Theme() { ShortName ="Classic", FullName = "/PresentationFramework.Classic;component/themes/Classic.xaml"},
                new Theme() { ShortName ="Aero", FullName = "/PresentationFramework.Aero;component/themes/Aero.NormalColor.xaml"},
                new Theme() { ShortName ="AeroLite", FullName = "/PresentationFramework.AeroLite;component/themes/aerolite.normalcolor.xaml"},
                new Theme() { ShortName ="Luna normalcolor", FullName = "/PresentationFramework.Luna;component/themes/luna.normalcolor.xaml"},
                new Theme() { ShortName ="Luna homestead", FullName = "/PresentationFramework.Luna;component/themes/luna.homestead.xaml"},
                new Theme() { ShortName ="Luna metallic", FullName = "/PresentationFramework.Luna;component/themes/luna.metallic.xaml"},
                new Theme() { ShortName ="Luna.Metallic", FullName = "/PresentationFramework.Luna.Metallic;component/themes/Luna.Metallic.xaml"},
                new Theme() { ShortName ="Zune", FullName = "/PresentationFramework.Zune;component/themes/Zune.NormalColor.xaml"},
                new Theme() { ShortName ="Royale", FullName = "/PresentationFramework.Royale;component/themes/royale.normalcolor.xaml"}
            };

        private Theme _selectTheme;

        public Theme SelectedTheme
        {
            get { return _selectTheme; }
            set
            {
                _selectTheme = value;
                OnChanged("SelectedTheme");
                ChangeTheme(_selectTheme);
            }
        }

        private void ChangeTheme(Theme _SelectTheme)
        {
            this.LoadTheme(_SelectTheme.FullName);
        }

        private ObservableCollection<Theme> _themes;

        public ObservableCollection<Theme> Themes
        {
            get { return _themes; }
            set { _themes = value; OnChanged("Themes"); }
        }

        public void OnChanged(string name)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(name));
        }

        #endregion Themes

        // ID constants
        private const Int32 m_baseID = 1001;

        public void LoadTheme(string themePath)
        {
            // очищаем перед загрузкой темы
            if (this.Resources.MergedDictionaries.Count > 1)
            {
                int count = this.Resources.MergedDictionaries.Count - 1;
                for (int i = 1; i <= count; i++)
                    this.Resources.MergedDictionaries.RemoveAt(1);
            }
            // загружаем необходимые для работы ресурсы
            // загружаем тему
            try
            {
                //this.Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = new Uri("/WpfApplication1;component/Themes/Graph.xaml", UriKind.RelativeOrAbsolute) });
                this.Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = new Uri(themePath, UriKind.RelativeOrAbsolute) });
            }
            catch (Exception)
            {
                MessageBox.Show("Не удалось применить тему.", App.Current.MainWindow.Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void MakeMenu()
        {
            if (Themes == null)
                Themes = new ObservableCollection<Theme>();
            foreach (var themeItem in themesList)
                Themes.Add(themeItem);

            if (Directory.Exists("Themes"))
            {
                FileInfo[] localthemes = new System.IO.DirectoryInfo("Themes").GetFiles();
                foreach (var item in localthemes)
                {
                    Themes.Add(new Theme { ShortName = item.Name, FullName = item.FullName });
                }
            }

            //Create a new submenu structure
            IntPtr hMenu = SystemMenu.AddSysMenuSubMenu();
            if (hMenu != IntPtr.Zero)
            {
                // Build submenu items of hMenu
                uint index = 0;
                for (int i = 0; i < Themes.Count; i++)
                {
                    SystemMenu.AddSysMenuSubItem(Themes[i].ShortName, index, m_baseID + index, hMenu);
                    index++;
                }
                // Now add to main system menu (position 6)
                SystemMenu.AddSysMenuItem("Визуальная тема", 0, 6, hMenu);
                SystemMenu.AddSysMenuItem("-", 0, 7, IntPtr.Zero);
            }

            SelectedTheme = Themes[0];

            // Attach our WndProc handler to this Window
            HwndSource source = HwndSource.FromHwnd(new WindowInteropHelper(this).Handle);
            source.AddHook(new HwndSourceHook(WndProc));
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            // Check if a System Command has been executed
            if (msg == (int)SystemMenu.WindowMessages.wmSysCommand)
            {
                int menuID = wParam.ToInt32();

                if (menuID <= (m_baseID + this.Themes.Count()))
                    if (menuID >= m_baseID)
                        this.SelectedTheme = Themes[menuID - m_baseID];
            }

            return IntPtr.Zero;
        }
    }

    public class Theme
    {
        public string ShortName { get; set; }

        public string FullName { get; set; }
    }
}