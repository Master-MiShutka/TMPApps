using System.Collections.Generic;
using TMP.ARMTES.Model;

namespace TMP.ARMTES.Controls.DataViews
{
    /// <summary>
    /// Interaction logic for BillingExportApi.xaml
    /// </summary>
    public partial class BillingExportApi : DataViewer
    {
        public BillingExportApi() : base()
        {
            SetResourceReference(System.Windows.Controls.Control.StyleProperty, typeof(DataViewer));

            InitializeComponent();

            DataContext = this;
        }
        protected override void Start()
        {
            DetailedStatus = "GetSubscriberResItems";
            {
                PageResult<SubscriberResItem> model = null;
                model = Webservice.GetDataFromArmtes<PageResult<SubscriberResItem>>("api/BillingExportApi/GetSubscriberResItems");
                if (model != null && model.Count > 0)
                {
                    SubscriberResItems = model.Items;
                    RaisePropertyChanged("SubscriberResItems");
                }
            }

            DetailedStatus = "GetResApps";
            {
                PageResult<ResApp> model = null;
                model = Webservice.GetDataFromArmtes<PageResult<ResApp>>("api/BillingExportApi/GetResApp");
                if (model != null && model.Count > 0)
                {
                    ResApps = model.Items;
                    RaisePropertyChanged("ResApps");
                }
            }

            DetailedStatus = "GetUnitedReports";
            {
                PageResult<UnitedReportViewItem> model = null;
                model = Webservice.GetDataFromArmtes<PageResult<UnitedReportViewItem>>("api/BillingExportApi/GetUnitedReport");
                if (model != null && model.Count > 0)
                {
                    UnitedReports = model.Items;
                    RaisePropertyChanged("UnitedReports");
                }
            }
        }
        /// <summary>
        /// Gets or sets the SubscriberResItems
        /// </summary>
        public ICollection<SubscriberResItem> SubscriberResItems { get; set; }

        /// <summary>
        /// Gets the SubscriberResItemsColumns
        /// </summary>
        public ICollection<ColumnDescriptor> SubscriberResItemsColumns => new ColumnDescriptor[]
        {
            new ColumnDescriptor("Наименование РЭС", "ResName"),
            new ColumnDescriptor("Количество", "SubscribersCount")
        };

        /// <summary>
        /// Gets or sets the ResApps
        /// </summary>
        public ICollection<ResApp> ResApps { get; set; }

        /// <summary>
        /// Gets the ResAppsColumns
        /// </summary>
        public ICollection<ColumnDescriptor> ResAppsColumns => new ColumnDescriptor[]
        {
            new ColumnDescriptor("Es", "Es"),
            new ColumnDescriptor("Наименование РЭС", "Res"),
            new ColumnDescriptor("Nn", "Nn")
        };

        /// <summary>
        /// Gets or sets the UnitedReports
        /// </summary>
        public ICollection<UnitedReportViewItem> UnitedReports { get; set; }

        /// <summary>
        /// Gets the UnitedReportsColumns
        /// </summary>
        public ICollection<ColumnDescriptor> UnitedReportsColumns => new ColumnDescriptor[]
        {
            new ColumnDescriptor("РЭС", "ResName"),
            new ColumnDescriptor("Город", "City"),
            new ColumnDescriptor("Улица", "Street"),
            new ColumnDescriptor("Дом", "House"),
            new ColumnDescriptor("Расчетные точки", "AccountingPointsCount"),
            new ColumnDescriptor("Все показания", "AllIndications"),
            new ColumnDescriptor("Показания по ЛС", "PersonalAccountIndications"),
            new ColumnDescriptor("Показания без ЛС", "WithoutPersonalAccountIndications"),
            new ColumnDescriptor("Ошибки", "ErrorsWithPersonalAccounts"),
            new ColumnDescriptor("Баланс", "HousesOnBalance")
        };

        protected override void Init()
        {
            Items = new DataViewList()
            {
                new DataItem() { Header = "SubscriberResItems", Items = SubscriberResItems, Columns = SubscriberResItemsColumns },
                new DataItem() { Header = "ResApps", Items = ResApps, Columns = ResAppsColumns },
                new DataItem() { Header = "UnitedReports", Items = UnitedReports, Columns = UnitedReportsColumns }
            };
        }

    }
}
