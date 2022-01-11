using System;
using System.Windows.Controls;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Collections.ObjectModel;

namespace TMP.Work.Emcos.Controls
{
    using Model;    
    using TMP.Shared.Commands;
    using TMPApplication.WpfDialogs.Contracts;

    /// <summary>
    /// Interaction logic for PointsEditor.xaml
    /// </summary>
    public partial class PointsEditor : UserControl, INotifyPropertyChanged
    {
        #region Fields

        const string TITLE = "Редактор списка точек";
        IWindowWithDialogs _window;

        EmcosTestWebService.ServiceSoapClient _emcosWebServiceClient;

        HierarchicalEmcosPointCollection _emcosFromSiteModel, _emcosPoints, _otherPoints;
        IHierarchicalEmcosPoint _emcosPointsFromSiteSelectedValue, _emcosPointsSelectedValue;

        /// <summary>
        /// ИД корневой точки с которой строится дерево точек, 60 - Ошмянские ЭС
        /// Id 52 Белэнерго Code 1
        /// Id 53 РУП Гродноэнерго Code 14
        /// Id 60 Ошмянские ЭС Code 143
        /// </summary>
        private readonly EmcosPoint DEFAULT_ROOT_EMCOS_GROUP = new EmcosPoint { Id = 60, TypeCode = "FES", Name = "Ошмянские ЭС" };
        private EmcosPoint _rootEmcosGroup;

        #endregion

        #region Constructor and desstructor

        public PointsEditor(IEnumerable<IHierarchicalEmcosPoint> balancePoints, IEnumerable<IHierarchicalEmcosPoint> otherPoints, IWindowWithDialogs window)
        {
            _rootEmcosGroup = DEFAULT_ROOT_EMCOS_GROUP;
            _window = window;

            EmcosPoints = new HierarchicalEmcosPointCollection(null, balancePoints);

            OtherPoints = new HierarchicalEmcosPointCollection(null, otherPoints);

            InitializeComponent();
            DataContext = this;

            NewPoints = new List<IHierarchicalEmcosPoint>();

            // настройка сервиса
            System.ServiceModel.Channels.Binding binding = new System.ServiceModel.BasicHttpBinding()
            {
                MaxBufferSize = 10240000,
                MaxReceivedMessageSize = 10240000,
                Name = "ServiceSoap"
            };
            _emcosWebServiceClient = new EmcosTestWebService.ServiceSoapClient(binding,
                new System.ServiceModel.EndpointAddress(String.Format("http://{0}/{1}",
                    EmcosSettings.Default.ServerAddress,
                    EmcosSettings.Default.WebServiceName)));

            this.Loaded += (s, e) =>
            {
                //await LoadPointsFromServiceAsync();
            };

            CommandUpdate = new DelegateCommand(async () =>
            {
                await LoadPointsFromServiceAsync();
            });

            CommandDelete = new DelegateCommand(() =>
            {
                var dialog = _window.DialogQuestion(String.Format("Точка '{0}' будет удалена. Вы уверены, что хотите продолжить?", _emcosPointsSelectedValue.Name));
                dialog.YesText = "Да, удалить";
                dialog.NoText = "Отменить";
                dialog.Yes = () =>
                {
                    _emcosPointsSelectedValue.Parent.Children.Remove(_emcosPointsSelectedValue);

                    if (NewPoints.Contains(_emcosPointsSelectedValue))
                        NewPoints.Remove(_emcosPointsSelectedValue);

                    if (DeletedPoints.Contains(_emcosPointsSelectedValue) == false)
                        DeletedPoints.Add(_emcosPointsSelectedValue);
                };
            }, (o) => _emcosPointsSelectedValue != null && _emcosPointsSelectedValue.Parent != null);

            CommandMoveUp = new DelegateCommand(() =>
            {
                int index = _emcosPointsSelectedValue.Parent.Children.IndexOf(_emcosPointsSelectedValue);
                _emcosPointsSelectedValue.Parent.Children.Move(index, index - 1);
            }, 
            (o) => _emcosPointsSelectedValue != null && 
                   _emcosPointsSelectedValue.Parent != null &&
                   _emcosPointsSelectedValue.Parent.Children.IndexOf(_emcosPointsSelectedValue) > 0
                   );

            CommandMoveDown = new DelegateCommand(() =>
            {
                int index = _emcosPointsSelectedValue.Parent.Children.IndexOf(_emcosPointsSelectedValue);
                _emcosPointsSelectedValue.Parent.Children.Move(index, index + 1);
            }, 
            (o) => _emcosPointsSelectedValue != null && 
                   _emcosPointsSelectedValue.Parent != null && 
                   _emcosPointsSelectedValue.Parent.Children.IndexOf(_emcosPointsSelectedValue) < _emcosPointsSelectedValue.Parent.ChildrenCount - 1);

            CommandClear = new DelegateCommand(() =>
            {
                var dialog = _window.DialogQuestion("Список точек будет очищен. Вы уверены, что хотите продолжить?");
                dialog.YesText = "Да, очистить";
                dialog.NoText = "Отменить";
                dialog.Yes = () =>
                {
                    DeletedPoints.Clear();
                    DeletedPoints.AddRange(EmcosPoints.FlatItemsList);
                    EmcosPoints.Clear();
                    NewPoints.Clear();
                };
            }, (o) => EmcosPoints != null && EmcosPoints.Count > 0);

            CommandAdd = new DelegateCommand(() =>
            {
                if (_emcosPointsSelectedValue != null)
                    _emcosPointsSelectedValue.Children.Add(_emcosPointsFromSiteSelectedValue);
                else
                    EmcosPoints.Add(_emcosPointsFromSiteSelectedValue);

                if (NewPoints.Contains(_emcosPointsFromSiteSelectedValue) == false)
                    NewPoints.Add(_emcosPointsFromSiteSelectedValue);
            }, (o) => _emcosPointsFromSiteSelectedValue != null);

            CommandBalanceFormula = new DelegateCommand(() =>
            {
                BalanceFormulaEditor ctrl = new BalanceFormulaEditor();
                ctrl.DataContext = Repository.Instance.GetGroupBalanceFormula(_emcosPointsSelectedValue.Id);

                var dialog = _window.DialogCustom(ctrl, TMPApplication.WpfDialogs.DialogMode.YesNo);

                dialog.YesText = "Записать";
                dialog.NoText = "Отменить";
                dialog.Yes = () =>
                {
                    ;
                };
            }, (o) => _emcosPointsSelectedValue != null && _emcosPointsSelectedValue.IsGroup);
        }

        #endregion

        #region Command and Properties
        public ICommand CommandAdd { get; private set; }
        public ICommand CommandUpdate { get; private set; }
        public ICommand CommandDelete { get; private set; }
        public ICommand CommandMoveUp { get; private set; }
        public ICommand CommandMoveDown { get; private set; }
        public ICommand CommandClear { get; private set; }

        public ICommand CommandBalanceFormula { get; private set; }
        
        /// <summary>
        /// Коллекция точек из сервиса
        /// </summary>
        public HierarchicalEmcosPointCollection EmcosPointsFromSite
        {
            get { return _emcosFromSiteModel; }
            private set
            {
                SetProperty(ref _emcosFromSiteModel, value);
            }
        }
        /// <summary>
        /// Коллекция точек для расчёта баланса энергии
        /// </summary>
        public HierarchicalEmcosPointCollection EmcosPoints
        {
            get { return _emcosPoints; }
            private set
            {
                SetProperty(ref _emcosPoints, value);
            }
        }
        // <summary>
        /// Коллекция прочих точек
        /// </summary>
        public HierarchicalEmcosPointCollection OtherPoints
        {
            get { return _otherPoints; }
            private set
            {
                SetProperty(ref _otherPoints, value);
            }
        }
        /// <summary>
        /// Выбранная точка в коллекции из сервиса
        /// </summary>
        public IHierarchicalEmcosPoint EmcosPointsFromSiteSelectedValue
        {
            get { return _emcosPointsFromSiteSelectedValue; }
            set
            {
                SetProperty(ref _emcosPointsFromSiteSelectedValue, value);
            }
        }
        /// <summary>
        /// Выбранная точка в конфигурации
        /// </summary>
        public IHierarchicalEmcosPoint EmcosPointsSelectedValue
        {
            get { return _emcosPointsSelectedValue; }
            set
            {
                SetProperty(ref _emcosPointsSelectedValue, value);
            }
        }
        /// <summary>
        /// Список новых точек в конфигурации
        /// </summary>
        public List<IHierarchicalEmcosPoint> NewPoints { get; private set; }
        /// <summary>
        /// Список удаленных точек из конфигурации
        /// </summary>
        public List<IHierarchicalEmcosPoint> DeletedPoints { get; private set; }

        #endregion

        #region Private Methods

        private async Task LoadPointsFromServiceAsync()
        {
            const string ErrorMessage = "Получение списка точек от сервиса - ошибка:\n{0}";
            IDialog dialog = null;
            try
            {
                dialog = _window.DialogWaitingScreen("Получение данных ...", indeterminate: true, mode: TMPApplication.WpfDialogs.DialogMode.Cancel);
                dialog.Show();
                var source = await FillPointsTree(_rootEmcosGroup);
                
                if (source.Count == 0)
                {
                    dialog.Close();
                    _window.ShowDialogWarning("Список пуст!", TITLE);
                    EmcosPointsFromSite = null;
                }
                else
                {
                    dialog.Caption = "Создание модели";
                    EmcosSiteWrapperApp.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background, new System.Threading.ThreadStart(delegate { }));

                    EmcosPointsFromSite = new HierarchicalEmcosPointCollection(null, source);
                    dialog.Caption = "Поиск изменений";
                    EmcosSiteWrapperApp.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background, new System.Threading.ThreadStart(delegate { }));

                    // поиск новых точек
                    void checkNewItems(IEnumerable<IHierarchicalEmcosPoint> sourcePoints, IEnumerable<IHierarchicalEmcosPoint> destinationPoints)
                    {
                        foreach (var point in sourcePoints)
                        {
                            IHierarchicalEmcosPoint p = destinationPoints.Where(i => i.Id == point.Id).FirstOrDefault();
                            if (p == null)
                            {
                                point.Tag = "NEW";
                            }
                            else
                            {
                                point.Tag = null;
                                if (point.HasChildren)
                                    checkNewItems(point.Children, p.Children);
                            }
                        }
                    }
                    checkNewItems(EmcosPointsFromSite, EmcosPoints);
                    
                    // поиск удаленных точек
                    void checkDeletedItems(IEnumerable<IHierarchicalEmcosPoint> sourcePoints, IEnumerable<IHierarchicalEmcosPoint> destinationPoints)
                    {
                        foreach (var point in sourcePoints)
                        {
                            IHierarchicalEmcosPoint p = destinationPoints.Where(i => i.Id == point.Id).FirstOrDefault();
                            if (p == null)
                            {
                                point.Tag = "DELETED";
                            }
                            else
                            {
                                point.Tag = null;
                                if (point.HasChildren)
                                    checkDeletedItems(point.Children, p.Children);
                            }
                        }
                    }
                    checkDeletedItems(EmcosPoints, EmcosPointsFromSite);

                    dialog.Close();
                }
            }
            catch (Exception ex)
            {
                EmcosSiteWrapperApp.Log("Получение списка точек от сервиса - ошибка");
                if (dialog != null)
                    dialog.Close();
                _window.ShowDialogError(String.Format(ErrorMessage, EmcosSiteWrapperApp.GetExceptionDetails(ex)));
            }
        }

        private IList<IHierarchicalEmcosPoint> ParsePointsList(string data, IHierarchicalEmcosPoint parent)
        {
            var records = Utils.ParseRecords(data);
            if (records == null)
                return new List<IHierarchicalEmcosPoint>();

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
                        case "POINT_ID":
                            int.TryParse(nvc[i], out intValue);
                            element.Id = intValue;
                            break;
                        case "GR_NAME":
                        case "POINT_NAME":
                            element.Name = nvc[i];
                            break;
                        case "ECP_NAME":
                            Model.EmcosPointElement pe = (element as Model.EmcosPointElement);
                            pe.EcpName = nvc[i];
                            break;
                        case "TYPE":
                            Model.ElementTypes type;
                            if (Enum.TryParse<Model.ElementTypes>(nvc[i], out type) == false)
                                type = Model.ElementTypes.GROUP;
                            element.Type = type;
                            break;
                        case "GR_TYPE_CODE":
                        case "POINT_TYPE_CODE":
                            element.TypeCode = nvc[i];
                            break;
                    }
                    #endregion
                }
                list.Add(element);
            }
            IList<IHierarchicalEmcosPoint> points = list.Select(i => new HierarchicalEmcosPoint
            {
                Id = i.Id,
                Name = i.Name,
                //IsGroup = i.Type == Model.ElementTypes.GROUP,
                TypeCode = i.TypeCode,
                EcpName = i is Model.EmcosPointElement ? (i as Model.EmcosPointElement).EcpName : String.Empty,
                ElementType = i.Type,
                IsChecked = false,
                Parent = parent
            }).ToList<IHierarchicalEmcosPoint>();
            return points;
        }

        private readonly int maxItemsCount = 10000;
        private int _index = 0;
        private async Task<IList<IHierarchicalEmcosPoint>> FillPointsTree(IHierarchicalEmcosPoint point)
        {
            var table = _emcosWebServiceClient.GetGRInfo("PSDTU_SERVER", point.Id.ToString());

            string data = await EmcosSiteWrapper.Instance.ExecuteFunction(EmcosSiteWrapper.Instance.GetAPointsAsync, point.Id.ToString());

            IList<IHierarchicalEmcosPoint> list = (String.IsNullOrEmpty(data)) ? new List<IHierarchicalEmcosPoint>() : ParsePointsList(data, point);

            if (list != null && list.Count > 0)
                for (int i = 0; i < list.Count; i++)
                {
                    EmcosSiteWrapperApp.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background, new System.Threading.ThreadStart(delegate { }));
                    if (list[i].ElementType == ElementTypes.GROUP)
                    {
                        IList<IHierarchicalEmcosPoint> childs = await FillPointsTree(list[i]);
                        foreach (var child in childs)
                            list[i].Children.Add(child);
                        _index++;
                        if (_index > maxItemsCount) throw new OverflowException("Превышено максимальное количество элементов в дереве - " + maxItemsCount);
                    }
                }
            return list;
        }

        #endregion

        #region INotifyPropertyChanged Members

        #region Debugging Aides

        protected virtual bool ThrowOnInvalidPropertyName { get; private set; }

        [Conditional("DEBUG")]
        [DebuggerStepThrough]
        public void VerifyPropertyName(string propertyName)
        {
            // Verify that the property name matches a real,
            // public, instance property on this object.
            if (TypeDescriptor.GetProperties(this)[propertyName] == null)
            {
                string msg = "Invalid property name: " + propertyName;

                if (this.ThrowOnInvalidPropertyName)
                    throw new Exception(msg);
                else
                    Debug.Fail(msg);
            }
        }
        #endregion Debugging Aides

        public event PropertyChangedEventHandler PropertyChanged;

        public bool SetProperty<T>(ref T storage, T value, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            if (Equals(storage, value))
            {
                return false;
            }

            storage = value;
            this.OnPropertyChanged(propertyName);
            return true;
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            this.VerifyPropertyName(propertyName);

            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
            {
                var e = new PropertyChangedEventArgs(propertyName);
                handler(this, e);
            }
        }

        #endregion INotifyPropertyChanged Members
    }
}
