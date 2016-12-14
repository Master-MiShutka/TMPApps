using TMP.DWRES.Graph;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using System.Threading;
using Model = TMP.DWRES.Objects;

using TMP.DWRES.DB;
using TMP.Shared;

namespace TMP.DWRES.ViewModel
{
    public partial class MainWindowViewModel : PropertyChangedBase
    {
        #region private fields

        private DBConnectionParams _dbConnectionParams = null;
        private dWRESdbHelper _dwh = new dWRESdbHelper();
        private bool _dbLoaded = false;

        dWRESdbHelper.FiderScheme _graphScheme;

        internal enum GraphVariant
        {
            Fider,
            Substation
        }
        private GraphVariant _currentGraphVariant = GraphVariant.Fider;

        #endregion

        #region Public Properties

        public DBConnectionParams DBConnectionParams
        {
            get { return _dbConnectionParams; }
            set { SetProp<DBConnectionParams>(ref _dbConnectionParams, value, "DBConnectionParams"); }
        }
        public bool DBLoaded
        {
            get { return _dbLoaded; }
            set { SetProp<bool>(ref _dbLoaded, value, "DBLoaded"); }
        }        

        public bool RecentDBExists
        {
            get { return System.IO.File.Exists(UserPrefs.FileName); }
        }
        public string DBFileName
        {
            get { return UserPrefs.FileName; }
            set { UserPrefs.FileName = value; RaisePropertyChanged("DBFileName"); }
        }
        
        #endregion

        #region Public Methods

        public void BuildGraph()
        {
            switch (_currentGraphVariant)
            {
                case GraphVariant.Fider:
                    BuildFiderGraph();
                    break;
                case GraphVariant.Substation:
                    BuildSubstationGraph();
                    break;
                default:
                    break;
            }
        }

        #endregion

        #region Private Methods

        private FiderGraph MakeGraph(ICollection<FiderGraphVertex> vertities, ICollection<FiderGraphEdge> edges)
        {
            FiderGraph result = new FiderGraph();
            foreach (FiderGraphVertex vertex in vertities)
                result.AddVertex(vertex);

            foreach (FiderGraphEdge edge in edges)
                result.AddEdge(edge);

            return result;
        }
        private FiderGraph GetFiderGraph(TMP.DWRES.Objects.Fider fider)
        {
            // получаем схему фидера
            _graphScheme = _dwh.GetFiderLines(fider);

            // строим граф фидера
            return MakeGraph(_graphScheme.GraphVertities, _graphScheme.GraphEdges);
        }
        private FiderGraph GetSubstationGraph(TMP.DWRES.Objects.Substation substation)
        {
            // получаем схему подстанции
            _graphScheme = _dwh.GetSubstationLines(substation);

            // строим граф подстанции
            return MakeGraph(_graphScheme.GraphVertities, _graphScheme.GraphEdges);
        }

        private void BuildFiderGraph()
        {
            if (SelectedFider == null)
                return;

            State = StateType.Building;
            MainWindow.Cursor = Cursors.Wait;

            ThreadStart threadStart = delegate ()
            {
                try
                {
                    TMP.DWRES.Graph.FiderGraph graph = GetFiderGraph(SelectedFider);
                    ModelGraph = graph;
                }
                finally
                {
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        // получаем участки
                        Lines = _dwh.GetLines(SelectedFider.ID);
                        LinesWithNames = _graphScheme.Lines;

                        // сохраняем список тп
                        BuildTPList();

                        MainWindow.Cursor = Cursors.Arrow;
                        State = StateType.Ready;
                        _currentGraphVariant = GraphVariant.Fider;
                    }));
                }
            };

            Thread thread = new Thread(threadStart);
            thread.Name = "Build fider graph";
            thread.Priority = ThreadPriority.BelowNormal;
            thread.Start();
        }

        private void BuildSubstationGraph()
        {
            if (SelectedSubstation == null) return;

            Model.Substation selectedPst = SelectedSubstation;

            SelectedFider = null;

            MainWindow.Cursor = Cursors.Wait;
            State = StateType.Building;          

            ThreadStart threadStart = delegate ()
            {
                try
                {
                    TMP.DWRES.Graph.FiderGraph substationGraph = GetSubstationGraph(selectedPst);
                    ModelGraph = substationGraph;
                }
                finally
                {
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        // получаем участки
                        LinesWithNames = Lines;

                        // сохраняем список тп
                        BuildTPList();

                        MainWindow.Cursor = Cursors.Arrow;
                        State = StateType.Ready;

                        _currentGraphVariant = GraphVariant.Substation;
                    }));
                }
            };

            Thread thread = new Thread(threadStart);
            thread.Name = "Build substation graph";
            thread.Priority = ThreadPriority.BelowNormal;
            thread.Start();
        }

        private void BuildTPList()
        {
            if (_tps == null)
                _tps = new ObservableCollection<Model.Tp>();
            _tps.Clear();
            ICollection<Graph.FiderGraphVertex> graphVertities = _graphScheme.GraphVertities;
            List<Model.Tp> list = new List<Objects.Tp>();
            foreach (Graph.FiderGraphVertex vertex in graphVertities)
            {
                if (vertex.Type == GraphVertexType.Transformer)
                {
                    Model.Tp tp = new Objects.Tp();
                    tp.ID = vertex.ID;

                    string[] parts = vertex.Name.Split(new char[] { '-', '/' });
                    if (parts.Length == 0)
                        System.Diagnostics.Debugger.Break();
                    tp.Name = String.Format("{0,-5} - {1}", parts[0].Trim(), parts[1].Trim());
                    list.Add(tp);
                }
            }

            list.Sort((x, y) =>
            {
                string[] xparts = x.Name.Split(new char[] { '-' });
                if (xparts.Length == 0)
                    System.Diagnostics.Debugger.Break();
                String xname = xparts[1].Trim();
                string[] yparts = y.Name.Split(new char[] { '-' });
                if (yparts.Length == 0)
                    System.Diagnostics.Debugger.Break();
                String yname = yparts[1].Trim();

                if (xname.Length <= 1) return 1;
                if (yname.Length <= 1) return -1;

                int xnumber = 0, ynumber = 0;
                if (Int32.TryParse(xname, out xnumber) && Int32.TryParse(yname, out ynumber))
                    return xnumber.CompareTo(ynumber);
                if (Int32.TryParse(xname, out xnumber) && Int32.TryParse(yname.Substring(0, yname.Length - 2), out ynumber))
                    return xnumber.CompareTo(ynumber);
                if (Int32.TryParse(xname.Substring(0, xname.Length - 2), out xnumber) && Int32.TryParse(yname, out ynumber))
                    return xnumber.CompareTo(ynumber);
                if (Int32.TryParse(xname.Substring(0, xname.Length - 2), out xnumber) && Int32.TryParse(yname.Substring(0, yname.Length - 2), out ynumber))
                    return xnumber.CompareTo(ynumber);
                return xname.CompareTo(yname);
            });

            _tps = new ObservableCollection<Objects.Tp>(list);
            RaisePropertyChanged("TPs");
        }

        /// <summary>
        /// Подключение к базе данных
        /// </summary>
        /// <param name="connectToServer">флаг, указывающий на подключение к серверу</param>
        /// <returns></returns>
        private bool SelectDBAndConnect(bool connectToServer = false)
        {
            bool result = true;

            try
            {
                MainWindow.TaskbarItemInfo.Description = "Загрузка данных ...";
                MainWindow.TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Indeterminate;

                if (DBLoaded)
                {
                    DBLoaded = false;
                    State = StateType.NoData;
                    _dwh.Close();
                    //result = false;
                }
                if (connectToServer == false)
                {
                    if (String.IsNullOrEmpty(DBFileName))
                    {
                        if (!SelectDBFile())
                            result = false;
                    }
                    else
                        if (!System.IO.File.Exists(DBFileName))
                    {
                        MainWindow.TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Error;
                        MessageBoxResult mresult = MessageBox.Show(
                            String.Format("Ошибка!\nНе удалось открыть базу данных:\n{0}\nВыбрать другую базу данных?", DBFileName),
                            MainTitle, MessageBoxButton.YesNo, MessageBoxImage.Information);
                        if (mresult == MessageBoxResult.Yes)
                        {
                            if (!SelectDBFile())
                                result = false;
                        }
                        else
                            result = false;
                    }
                }
                if (connectToServer) result = true;
                if (result)
                {
                    try
                    {
                        if (connectToServer)
                            _dwh.Init(DBConnectionParams);
                        else
                            _dwh.Init(DBFileName);
                        result = true;
                    }
                    catch (Exception ex)
                    {
                        MainWindow.TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Error;
                        MessageBoxResult mresult = MessageBox.Show(ex.Message, MainTitle, MessageBoxButton.OK, MessageBoxImage.Warning);
                        result = false;
                    }
                }
            }
            finally
            {
                MainWindow.TaskbarItemInfo.Description = String.Empty;
                MainWindow.TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.None;
            }
            if (result)
            {
                DBLoaded = true;
                State = StateType.Loaded;
                return true;
            }
            else
            {
                DBLoaded = false;
                State = StateType.NoData;
                return false;
            }
        }
        /// <summary>
        /// Выбор файла базы данных
        /// </summary>
        /// <returns></returns>
        private bool SelectDBFile()
        {
            try
            {
                MainWindow.TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Indeterminate;
                Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
                dlg.DefaultExt = ".fdb";
                dlg.Filter = "База данных приложения dWRES (.fdb)|*.fdb";
                dlg.RestoreDirectory = true;
                dlg.Title = "Выберите файл базы данных программы dWRES";

                Nullable<bool> result = dlg.ShowDialog();

                if (result == true)
                {
                    DBFileName = dlg.FileName;
                    return true;
                }
                else
                    return false;
            }
            finally
            {
                MainWindow.TaskbarItemInfo.Description = String.Empty;
                MainWindow.TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.None;
            }
        }

        #endregion
    }
}
