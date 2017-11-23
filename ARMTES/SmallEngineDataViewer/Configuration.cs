using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using TMP.ARMTES.Model;

namespace TMP.ARMTES
{
    internal class Configuration : INotifyPropertyChanged
    {
        private Window _mainWindow;
        private Task<Data> _task;

        public static Configuration Instance { get; internal set; }

        static Configuration()
        {
            Instance = new Configuration();
        }

        public Configuration()
        {
            if (Instance != null) return;
            if (SmallEngineViewerApp.Current.MainWindow != null)
                _mainWindow = SmallEngineViewerApp.Current.MainWindow;

            //Settings = new Data();
            DataViewsSettings = new Dictionary<string, object>();
            try
            {
                string fileNameFormat = "dataview-ConfigurationApi-AllConfigurations-{0}";
                foreach (string filename in System.IO.Directory.EnumerateFiles(AppDomain.CurrentDomain.BaseDirectory, string.Format(fileNameFormat, "*")))
                {
                    PageResult<ConfigurationContainer> model;
                    var serializer = new Newtonsoft.Json.JsonSerializer();
                    using (System.IO.FileStream fs = new System.IO.FileStream(filename, System.IO.FileMode.Open, System.IO.FileAccess.Read))
                    using (System.IO.StreamReader sr = new System.IO.StreamReader(fs))
                    using (var jsonTextReader = new Newtonsoft.Json.JsonTextReader(sr))
                    {
                        model = serializer.Deserialize<PageResult<ConfigurationContainer>>(jsonTextReader);
                    }
                    if (model != null && model.Count > 0)
                        DataViewsSettings.Add(System.IO.Path.GetFileNameWithoutExtension(filename), model.Items);
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.Fail(SmallEngineViewerApp.GetExceptionDetails(e));
            }
        }

        public Data Settings { get; set; }

        public void LoadData(Action callBack = null)
        {
            // путь к папке с исполняемым файлом программы
            string executionPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
            string fileName = System.IO.Path.Combine(executionPath, "data");
            if (System.IO.File.Exists(fileName))
            {
                ViewModel.IMainViewModel vm = _mainWindow.DataContext as ViewModel.IMainViewModel;

                vm.IsBusy = true;
                vm.Status = "загрузка данных";

                try
                {
                    _mainWindow.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        _mainWindow.TaskbarItemInfo = new System.Windows.Shell.TaskbarItemInfo();
                        _mainWindow.TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Indeterminate;
                    }));
                    _task = new Task<Data>(() => Serializer.GzJsonDeSerialize(fileName, e => System.Diagnostics.Debugger.Log(0, "ERROR", e.Message)));
                    _task.ContinueWith(t =>
                        {
                            _mainWindow.Dispatcher.BeginInvoke((Action)(() => _mainWindow.TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Error));
                            MessageBox.Show("Не удалось загрузить ранее сохраненные данные.\n" + SmallEngineViewerApp.GetExceptionDetails(t.Exception), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            _mainWindow.Dispatcher.BeginInvoke((Action)(() => _mainWindow.TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.None));
                            vm.IsBusy = false;
                            vm.Status = String.Empty;
                        }, TaskContinuationOptions.OnlyOnFaulted);
                    _task.ContinueWith(t =>
                        {
                            Settings = t.Result;
                            RaisePropertyChanged("Settings");
                            RaisePropertyChanged("Enterprises");
                            RaisePropertyChanged("Reses");
                            RaisePropertyChanged("FesWithReses");

                            vm.IsBusy = false;
                            vm.Status = String.Empty;
                            _mainWindow.Dispatcher.BeginInvoke((Action)(() => _mainWindow.TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.None));

                            if (callBack != null)
                                callBack();
                        }, TaskContinuationOptions.OnlyOnRanToCompletion);
                    _task.Start();
                }
                catch (Exception ex)
                {
#if DEBUG
                    SmallEngineViewerApp.ToDebug(ex);
#endif
                }

            }
        }
        // Серилизация и сохранение в файл
        public void SaveData(Action callBack = null)
        {
            // путь к папке с исполняемым файлом программы
            string executionPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
            string fileName = System.IO.Path.Combine(executionPath, "data");

            ViewModel.IMainViewModel vm = _mainWindow.DataContext as ViewModel.IMainViewModel;

            vm.IsBusy = true;
            vm.Status = "сохранение данных";

            try
            {
                _mainWindow.Dispatcher.BeginInvoke((Action)(() =>
                {
                    _mainWindow.TaskbarItemInfo = new System.Windows.Shell.TaskbarItemInfo();
                    _mainWindow.TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Indeterminate;
                }));
                var task = new Task(() => Serializer.GzJsonSerialize(Settings, fileName, e => System.Diagnostics.Debugger.Log(0, "ERROR", e.Message)));
                task.ContinueWith(t =>
                    {
                        _mainWindow.Dispatcher.BeginInvoke((Action)(() => _mainWindow.TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Error));
                        MessageBox.Show("Не удалось сохранить данные." + Environment.NewLine + SmallEngineViewerApp.GetExceptionDetails(t.Exception), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        _mainWindow.Dispatcher.BeginInvoke((Action)(() => _mainWindow.TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.None));
                        vm.IsBusy = false;
                        vm.Status = String.Empty;
                    }, TaskContinuationOptions.OnlyOnFaulted);
                task.ContinueWith(t =>
                    {
                        vm.IsBusy = false;
                        vm.Status = String.Empty;
                        _mainWindow.Dispatcher.BeginInvoke((Action)(() => _mainWindow.TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.None));

                        if (callBack != null)
                            callBack();

                    }, TaskContinuationOptions.OnlyOnRanToCompletion);
                task.Start();
            }
            catch (Exception ex)
            {
#if DEBUG
                SmallEngineViewerApp.ToDebug(ex);
#endif
            }
        }

        public IEnumerable<string> ResNames { get; private set; }

        private IEnumerable<EnterpriseViewItem> _enterprises;
        public IEnumerable<EnterpriseViewItem> Enterprises
        {
            get
            {
                if (Settings == null) return null;

                if (_enterprises == null && Settings.Enterprises != null)
                {
                    _enterprises = Settings.Enterprises;
                }
                else
                if (_enterprises == null)
                {
                    var enterprises = Webservice.GetEnterprises();
                    if (enterprises != null && enterprises.Count > 0)
                    {
                        var flat = enterprises.SelectMany(i => i.ChildEnterprises).ToList();
                        if (flat != null && flat.Count > 0)
                        {
                            var fes = flat.Where(i => i.EnterpriseName == "ОЭС");
                            if (fes != null && flat.Count == 1)
                            {
                                _enterprises = fes.SelectMany(i => i.ChildEnterprises).OrderBy(i => i.EnterpriseId);
                                ResNames = _enterprises.Select(r => r.EnterpriseName).ToArray();
                            }
                        }
                    }                    
                }
                return _enterprises;
            }
        }

        public IEnumerable<EnterpriseViewItem> Reses
        {
            get
            {
                if (Enterprises != null)
                    return Enterprises.Where(e => e.EnterpriseType == "РЭС");
                else
                    return new List<EnterpriseViewItem>();
            }
        }
        public IEnumerable<EnterpriseViewItem> FesWithReses
        {
            get
            {
                if (Enterprises != null)
                    return Enterprises
                        .Where(e => e.EnterpriseType == "ФЭС")
                        .Concat(Enterprises.Where(e => e.EnterpriseType == "РЭС"));
                else
                    return new List<EnterpriseViewItem>();
            }
        }

        public Dictionary<string, object> DataViewsSettings { get; set; }

        public IEnumerable<ConfigurationContainer> DevicesList
        {
            get
            {
                return (IList<ConfigurationContainer>)Instance.DataViewsSettings?["dataview-ConfigurationApi-AllConfigurations-ОЭС"];
            }
        }

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
