using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using TMP.UI.Controls.WPF;

namespace TMP.WORK.AramisChetchiki
{
    /// <summary>
    /// Interaction logic for PreferencesWindow.xaml
    /// </summary>
    public partial class PreferencesWindow : Window, INotifyPropertyChanged
    {
        public ViewModel.IMainViewModel MainViewModel { get; private set; }

        public PreferencesWindow(ViewModel.IMainViewModel mainViewModel)
        {
            if (mainViewModel == null)
                throw new ArgumentNullException("MainViewModel");

            MainViewModel = mainViewModel;
            InitializeComponent();
            DataContext = this;

            MainViewModel = mainViewModel;

            CommandAddDepartament = new DelegateCommand(() =>
            {
                if (Properties.Settings.Default.Departaments == null)
                    Properties.Settings.Default.Departaments = new ObservableCollection<Model.Departament>();

                if (Properties.Settings.Default.Departaments.Any(i => String.Equals(i.Path, DBPath)))
                {
                    MessageBox.Show("Это подразделение уже добавлено!", "Параметры", MessageBoxButton.OK, MessageBoxImage.Information);

                    return;
                }

                Model.Departament departament = new Model.Departament()
                {
                    Path = DBPath,
                    DataFileName = CreateFileName()
                };

                departament.Name = App.MainViewModel.GetDepartamentName(departament);
                if (String.IsNullOrWhiteSpace(departament.Name))
                    return;

                Properties.Settings.Default.Departaments.Add(departament);

                departament.IsSelected = true;

            }, () => String.IsNullOrWhiteSpace(DBPath) == false && System.IO.Directory.Exists(DBPath));

            CommandRemoveDepartament = new DelegateCommand(() =>
            {
                if (Properties.Settings.Default.Departaments != null)
                {
                    try
                    {
                        System.IO.File.Delete(SelectedDepartament.DataFileName);
                    }
                    catch { }
                    Properties.Settings.Default.Departaments.Remove(SelectedDepartament);
                }
            }, () => SelectedDepartament != null);

            CommandClearDepartamentsList = new DelegateCommand(() =>
            {
                if (Properties.Settings.Default.Departaments != null)
                {
                    try
                    {
                        foreach (Model.Departament d in Properties.Settings.Default.Departaments)
                            System.IO.File.Delete(d.DataFileName);
                    }
                    catch { }
                    Properties.Settings.Default.Departaments.Clear();
                }
            }, () => Properties.Settings.Default.Departaments != null && Properties.Settings.Default.Departaments.Count > 0);
            this.Closed += PreferencesWindow_Closed;
        }

        private void PreferencesWindow_Closed(object sender, EventArgs e)
        {
            // если указан путь к базе и список подразделений пуст, то создаем подразделение и добавляем в список
            if (String.IsNullOrWhiteSpace(DBPath) == false &&
                    ((Properties.Settings.Default.Departaments != null && Properties.Settings.Default.Departaments.Count == 0) || Properties.Settings.Default.Departaments == null))
                if (Properties.Settings.Default.Departaments.Any(i => String.Equals(i.Path, DBPath)) == false)
                    CommandAddDepartament.Execute(null);
        }

        private string CreateFileName()
        {
            string fileName = String.Empty;
            do
            {
                fileName = "Data-" + System.IO.Path.GetFileNameWithoutExtension(System.IO.Path.GetRandomFileName());
            } while (System.IO.File.Exists(fileName) == true);
            return fileName;
        }

        private void SelectFile_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();
            ofd.Title = "Выберите файл отчета";
            ofd.Filter = "Текстовые файлы |*.txt";
            ofd.DefaultExt = "*.txt";
            var r = ofd.ShowDialog();
            if (r != null && r == true)
            {
                //Go(ofd.FileName);
            }
        }

        public string DBPath { get; set; }
        public Model.Departament SelectedDepartament { get; set; }

        public ICommand CommandAddDepartament { get; }
        public ICommand CommandRemoveDepartament { get; }
        public ICommand CommandClearDepartamentsList { get; }

        #region INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;
        protected bool SetProperty<T>(ref T field, T value, string propertyName = null)
        {
            if (Equals(field, value)) { return false; }

            field = value;
            RaisePropertyChanged(propertyName);
            return true;
        }
        protected void RaisePropertyChanged(string propertyName = null)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)

        {
            PropertyChanged?.Invoke(this, e);
        }
        #endregion
    }
}
