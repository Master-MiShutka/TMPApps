using System.Collections.Generic;
using System.Collections.ObjectModel;
using TMP.ARMTES.Model;

namespace TMP.ARMTES.Controls.DataViews
{
    /// <summary>
    /// Interaction logic for ConfigurationApi.xaml
    /// </summary>
    public partial class ConfigurationApi :  DataViewer
    {
        private DataItem _allConfigurationsDataItem;
        public ConfigurationApi() : base()
        {
            SetResourceReference(System.Windows.Controls.Control.StyleProperty, typeof(DataViewer));

            InitializeComponent();

            DataContext = this;
        }
        protected override void Start()
        {
            DetailedStatus = "Подготовка отчёта по системам";
            if (ResId != 0)
            {
                PageResult<ConfigurationContainer> model = null;
                model = Webservice.GetDataFromArmtes<PageResult<ConfigurationContainer>>(
                    string.Format("api/ConfigurationApi/GetAllConfigurations?resId={0}&resName={1}", ResId, ResName));
                if (model != null && model.Count > 0)
                {
                    AllConfigurations = model.Items;
                    RaisePropertyChanged("AllConfigurations");
                }
            }
        }

        public int ResId => SelectedEnterprise == null ? 0 : SelectedEnterprise.EnterpriseId;
        public string ResName => SelectedEnterprise == null ? string.Empty : SelectedEnterprise.EnterpriseName;

        private EnterpriseViewItem _selectedEnterprise;
        public EnterpriseViewItem SelectedEnterprise
        {
            get => _selectedEnterprise;
            set
            {
                if (value != _selectedEnterprise)
                {
                    _selectedEnterprise = value;
                    RaisePropertyChanged("SelectedEnterprise");
                    string key = string.Format("dataview-ConfigurationApi-AllConfigurations-{0}", _selectedEnterprise.EnterpriseName);
                    if (Configuration.Instance.DataViewsSettings.ContainsKey(key))
                    {
                        AllConfigurations = (ICollection<ConfigurationContainer>)Configuration.Instance.DataViewsSettings[key];
                        Update();
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the AllConfigurations
        /// </summary>
        public ICollection<ConfigurationContainer> AllConfigurations { get; set; }

        /// <summary>
        /// Gets the AllConfigurationsColumns
        /// </summary>
        public ICollection<ColumnDescriptor> AllConfigurationsColumns => new ColumnDescriptor[]
        {
            new ColumnDescriptor("РЭС", "ResName"),
            new ColumnDescriptor("Модель УСПД", "CollectorModel"),
            new ColumnDescriptor("Версия УСПД", "CollectorSoftwareVersion"),
            new ColumnDescriptor("Зав. № УСПД", "CollectorSerial"),
            new ColumnDescriptor("Сетевой адрес УСПД", "CollectorNetworkAdress"),
            new ColumnDescriptor("№ телефона", "PhoneNumber"),
            new ColumnDescriptor("Модем", "ModemName"),
            new ColumnDescriptor("Зав. № модема", "ModemSerial"),
            new ColumnDescriptor("Регион", "RegionName"),
            new ColumnDescriptor("Сельсовет", "VillageUnionName"),
            new ColumnDescriptor("ТП", "TransformationSubstationName"),
            new ColumnDescriptor("ProjectName", "ProjectName"),
            new ColumnDescriptor("ТУ", "TechnicalConditionsName"),
            new ColumnDescriptor("Пароль УСПД", "CollectorsPasswords"),
            new ColumnDescriptor("Пароль счётчиков", "MetersPasswords"),
            new ColumnDescriptor("Счётчики", "MeterConfigurations",
                new ColumnDescriptor[]
                {
                    new ColumnDescriptor("№ п/п", "Number"),
                    new ColumnDescriptor("Город", "City"),
                    new ColumnDescriptor("Улица", "Street"),
                    new ColumnDescriptor("Дом", "House"),
                    new ColumnDescriptor("Квартира", "Flat"),
                    new ColumnDescriptor("Тип", "MeterType"),
                    new ColumnDescriptor("Зав. №", "MeterSerial"),
                    new ColumnDescriptor("Вирт. адрес", "MeterVirtualAdress"),
                    new ColumnDescriptor("Баланс", "MeterBelnoging"),
                    new ColumnDescriptor("№ интерфейса", "InterfaceNumber"),
                    new ColumnDescriptor("Физ. адрес УСПД", "MacCollectorModem"),
                    new ColumnDescriptor("Физ. адреса модема счётчика", "MacMeterModem"),
                    new ColumnDescriptor("Ктт", "TransformationCoefficient"),
                    new ColumnDescriptor("Потребитель", "SubscriberName"),
                    new ColumnDescriptor("ИД точки", "PersonalAccount"),
                    new ColumnDescriptor("Сетевой адрес", "MeterNetworkAdress")
                })
        };
        private void Update()
        {
            int index = -1;
            if (Items != null && _allConfigurationsDataItem != null)
                index = Items.IndexOf(_allConfigurationsDataItem);
            _allConfigurationsDataItem = new DataItem()
            {
                Header = "Перечень конфигураций " + SelectedEnterprise?.EnterpriseName,
                Items = AllConfigurations,
                Columns = AllConfigurationsColumns
            };
            if (index >= 0)
                Items[index] = _allConfigurationsDataItem;
        }

        protected override void Init()
        {
            Update();

            Items = new DataViewList()
            {
                _allConfigurationsDataItem
            };
        }
    }
}
