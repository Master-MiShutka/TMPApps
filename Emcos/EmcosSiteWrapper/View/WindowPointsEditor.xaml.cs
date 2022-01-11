using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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
using System.Windows.Shapes;

namespace TMP.Work.Emcos.View
{
    using Model;
    using System.Collections.ObjectModel;
    using TMP.Shared.Commands;

    /// <summary>
    /// Interaction logic for WindowPointsEditor.xaml
    /// </summary>
    public partial class WindowPointsEditor : TMPApplication.CustomWpfWindow.WindowWithDialogs, INotifyPropertyChanged
    {
        #region Constructor and desstructor

        public WindowPointsEditor(IList<IHierarchicalEmcosPoint> points)
        {
            _sourcePoints = points;
            EmcosPoints = new TreeModel(new EmcosPoint(name : "ROOT", children : points));

            InitializeComponent();
            DataContext = this;

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

            CloseCommand = new DelegateCommand(() =>
            {
                Close();
            });

            UpdateCommand = new DelegateCommand(async () =>
            {
                await LoadPointsFromServiceAsync();
            });

        }

        #endregion

        #region Command and Properties

        public bool IsGettingPointsFromService
        {
            get { return _isGettingPointsFromService; }
            private set
            {
                SetProperty(ref _isGettingPointsFromService, value);
            }
        }

        public ICommand CloseCommand { get; private set; }
        public ICommand UpdateCommand { get; private set; }

        public TreeModel EmcosFromSiteModel
        {
            get { return _emcosFromSiteModel; }
            private set
            {
                SetProperty(ref _emcosFromSiteModel, value);
            }
        }
        public TreeModel EmcosPoints
        {
            get { return _emcosPoints; }
            private set
            {
                SetProperty(ref _emcosPoints, value);
            }
        }

        #endregion

        #region Private Methods

        private async Task LoadPointsFromServiceAsync()
        {
            const string ErrorMessage = "Получение списка точек от сервиса - ошибка:\n{0}";
            try
            {

                IsGettingPointsFromService = true;

                var source = await FillPointsTree(_rootEmcosGroup);

                IsGettingPointsFromService = false;
                if (source.Count == 0)
                {
                    this.ShowDialogWarning("Список пуст!");
                    EmcosFromSiteModel = null;
                }
                else
                {
                    EmcosFromSiteModel = new TreeModel(new EmcosPoint("ROOT", source));
                }
            }
            catch (Exception ex)
            {
                EmcosSiteWrapperApp.Log("Получение списка точек от сервиса - ошибка");
                IsGettingPointsFromService = false;
                this.ShowDialogError(String.Format(ErrorMessage, EmcosSiteWrapperApp.GetExceptionDetails(ex)));
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
            IList<IHierarchicalEmcosPoint> points = list.Select(i => new EmcosPoint
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
            var table = _emcosWebServiceClient.GetPointInfo("PSDTU_SERVER", point.Id.ToString());
            string data = await EmcosSiteWrapper.Instance.GetAPointsAsync(point.Id.ToString());
            IList<IHierarchicalEmcosPoint> list = (String.IsNullOrEmpty(data)) ? new List<IHierarchicalEmcosPoint>() : ParsePointsList(data, point);

            if (list != null && list.Count > 0)
                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i].IsGroup)
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

        #region Fields

        EmcosTestWebService.ServiceSoapClient _emcosWebServiceClient;

        IList<IHierarchicalEmcosPoint> _sourcePoints;

        bool _isGettingPointsFromService = false;

        TreeModel _emcosFromSiteModel, _emcosPoints;

        /// <summary>
        /// ИД корневой точки с которой строится дерево точек, 60 - Ошмянские ЭС
        /// Id 52 Белэнерго Code 1
        /// Id 53 РУП Гродноэнерго Code 14
        /// Id 60 Ошмянские ЭС Code 143
        /// </summary>
        private readonly EmcosPoint DEFAULT_ROOT_EMCOS_GROUP = new EmcosPoint { Id = 60, TypeCode = "FES", Name = "Ошмянские ЭС" };
        private EmcosPoint _rootEmcosGroup;

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
