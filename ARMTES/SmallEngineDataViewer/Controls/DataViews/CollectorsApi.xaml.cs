using System.Collections.Generic;
using System.Collections.ObjectModel;
using TMP.ARMTES.Model;

namespace TMP.ARMTES.Controls.DataViews
{
    /// <summary>
    /// Interaction logic for CollectorsApi.xaml
    /// </summary>
    public partial class CollectorsApi :  DataViewer
    {
        public CollectorsApi() : base()
        {
            SetResourceReference(System.Windows.Controls.Control.StyleProperty, typeof(DataViewer));

            InitializeComponent();

            DataContext = this;
        }
        protected override void Start()
        {
            DetailedStatus = "Подготовка отчёта по системам";
            {
                PageResult<CollectorDeviceViewModel> model = null;
                model = Webservice.GetDataFromArmtes<PageResult<CollectorDeviceViewModel>>("api/CollectorsApi/Get");
                if (model != null && model.Count > 0)
                {
                    CollectorDevices = model.Items;
                    RaisePropertyChanged("CollectorDevices");
                }
            }
        }
        /// <summary>
        /// Gets or sets the CollectorDevices
        /// </summary>
        public ICollection<CollectorDeviceViewModel> CollectorDevices { get; set; }

        /// <summary>
        /// Gets the CollectorDevicesColumns
        /// </summary>
        public ICollection<ColumnDescriptor> CollectorDevicesColumns => new ColumnDescriptor[]
        {
            new ColumnDescriptor("ИД устройства", "DeviceId"),
            new ColumnDescriptor("Город", "City"),
            new ColumnDescriptor("Улица", "Street"),
            new ColumnDescriptor("Дом", "House"),
            new ColumnDescriptor("Канал связи", "PhoneNumber"),
            new ColumnDescriptor("Сетевой адрес", "NetworkAdress"),
            new ColumnDescriptor("Модель", "DeviceModel"),
            new ColumnDescriptor("Версия ПО", "DeviceFirmware"),
            new ColumnDescriptor("Полное название", "DeviceName"),
            new ColumnDescriptor("Тип системы", "SystemType")
        };

        protected override void Init()
        {
            Items = new DataViewList()
            {
                new DataItem() { Header = "Отчёт по системам", Items = CollectorDevices, Columns = CollectorDevicesColumns }
            };
        }

    }
}
