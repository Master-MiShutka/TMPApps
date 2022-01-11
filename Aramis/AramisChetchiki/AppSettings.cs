namespace TMP.WORK.AramisChetchiki
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using TMP.Shared;
    using TMP.WORK.AramisChetchiki.Model;

    public class AppSettings : PropertyChangedBase
    {
        #region Fields

        private readonly SummaryInfoFieldsCollection defaultSummaryInfoFields;
        private readonly ChangesOfMetersFieldsCollection defaultChangesOfMetersFields;
        private readonly MeterFieldsCollection defaultBaseViewColumns;
        private readonly MeterFieldsCollection defaultShortViewColumns;
        private readonly MeterFieldsCollection defaultDetailedViewColumns;
        private readonly MeterFieldsCollection defaultОплатаViewColumns;
        private readonly MeterFieldsCollection defaultПривязкаViewColumns;

        private double fontSize = 14.0;
        private string aramisDBPath = "d:\\aramis\\Disks\\OSHM\\aramis";
        private string dataFilesStorePath = string.Empty;
        private ChangesOfMetersFieldsCollection changesOfMetersFields;
        private Dictionary<ViewModel.IViewModel, List<DataGridWpf.DataGridWpfColumnViewModel>> viewModelsTableColumns;
        private SummaryInfoFieldsCollection summaryInfoFields;
        private InfoViewType selectedSummaryView = InfoViewType.ViewAsDiagram;
        private MeterFieldsCollection baseViewColumns;
        private MeterFieldsCollection shortViewColumns;
        private MeterFieldsCollection detailedViewColumns;
        private MeterFieldsCollection оплатаViewColumns;
        private MeterFieldsCollection привязкаViewColumns;
        private TableViewKinds selectedTableViewKind = TableViewKinds.BaseView;
        private TMPApplication.VisualTheme theme;
        private string lastUsedDataFileName;
        private bool isCellSelectionEnabled = false;
        private uint numberOfApartmentsInAnApartmentBuilding = 16;

        private Properties.Settings Settings => Properties.Settings.Default;

        #endregion

        private static AppSettings defaultInstance = new AppSettings();

        public static AppSettings Default
        {
            get
            {
                return defaultInstance;
            }
        }

        public AppSettings()
        {
            this.ViewModelsTableColumns = new Dictionary<ViewModel.IViewModel, List<DataGridWpf.DataGridWpfColumnViewModel>>();

            this.defaultSummaryInfoFields = new SummaryInfoFieldsCollection(
                ModelHelper.MeterSummaryInfoPropertiesCollection.Values.Select(i => i.Name).ToList());
            System.Diagnostics.Debug.Assert(this.defaultSummaryInfoFields.Count > 0, "DefaultSummaryInfoFields.Count <= 0");
            this.defaultChangesOfMetersFields = new ChangesOfMetersFieldsCollection(
                ModelHelper.ChangesOfMetersPropertiesCollection.Values.Select(i => i.Name).ToList());
            if (this.defaultChangesOfMetersFields.Count == 0)
            {
                System.Diagnostics.Debugger.Break();
            }

            this.defaultBaseViewColumns = Meter.GetSetOfColumns("BaseView");
            this.defaultShortViewColumns = Meter.GetSetOfColumns("ShortView");
            this.defaultDetailedViewColumns = Meter.GetSetOfColumns("DetailedView");
            this.defaultОплатаViewColumns = Meter.GetSetOfColumns("ОплатаView");
            this.defaultПривязкаViewColumns = Meter.GetSetOfColumns("ПривязкаView");

            if (this.SummaryInfoFields == null || this.SummaryInfoFields.Count == 0)
            {
                this.SummaryInfoFields = this.defaultSummaryInfoFields;
            }

            if (this.Settings.ChangesOfMetersFields == null || this.Settings.ChangesOfMetersFields.Count == 0)
            {
                this.Settings.ChangesOfMetersFields = this.defaultChangesOfMetersFields;
            }

            if (this.Settings.BaseViewColumns == null || this.Settings.BaseViewColumns.Count == 0)
            {
                this.Settings.BaseViewColumns = this.defaultBaseViewColumns;
            }

            if (this.Settings.ShortViewColumns == null || this.Settings.ShortViewColumns.Count == 0)
            {
                this.Settings.ShortViewColumns = this.defaultShortViewColumns;
            }

            if (this.Settings.DetailedViewColumns == null || this.Settings.DetailedViewColumns.Count == 0)
            {
                this.Settings.DetailedViewColumns = this.defaultDetailedViewColumns;
            }

            if (this.Settings.ОплатаViewColumns == null || this.Settings.ОплатаViewColumns.Count == 0)
            {
                this.Settings.ОплатаViewColumns = this.defaultОплатаViewColumns;
            }

            if (this.Settings.ПривязкаViewColumns == null || this.Settings.ПривязкаViewColumns.Count == 0)
            {
                this.Settings.ПривязкаViewColumns = this.defaultПривязкаViewColumns;
            }
        }

        #region Public properties

        public static readonly StringComparison StringComparisonMethod = StringComparison.Ordinal;

        public double FontSize { get => this.fontSize; set => this.SetProperty(ref this.fontSize, value); }

        public string AramisDBPath { get => this.aramisDBPath; set => this.SetProperty(ref this.aramisDBPath, value); }

        public string DataFilesStorePath { get => this.dataFilesStorePath; set => this.SetProperty(ref this.dataFilesStorePath, value); }

        public ChangesOfMetersFieldsCollection ChangesOfMetersFields { get => this.changesOfMetersFields; set => this.SetProperty(ref this.changesOfMetersFields, value); }

        public Dictionary<ViewModel.IViewModel, List<DataGridWpf.DataGridWpfColumnViewModel>> ViewModelsTableColumns { get => this.viewModelsTableColumns; set => this.SetProperty(ref this.viewModelsTableColumns, value); }

        public SummaryInfoFieldsCollection SummaryInfoFields { get => this.summaryInfoFields; set => this.SetProperty(ref this.summaryInfoFields, value); }

        public InfoViewType SelectedSummaryView { get => this.selectedSummaryView; set => this.SetProperty(ref this.selectedSummaryView, value); }

        public MeterFieldsCollection BaseViewColumns { get => this.baseViewColumns; set => this.SetProperty(ref this.baseViewColumns, value); }

        public MeterFieldsCollection ShortViewColumns { get => this.shortViewColumns; set => this.SetProperty(ref this.shortViewColumns, value); }

        public MeterFieldsCollection DetailedViewColumns { get => this.detailedViewColumns; set => this.SetProperty(ref this.detailedViewColumns, value); }

        public MeterFieldsCollection ОплатаViewColumns { get => this.оплатаViewColumns; set => this.SetProperty(ref this.оплатаViewColumns, value); }

        public MeterFieldsCollection ПривязкаViewColumns { get => this.привязкаViewColumns; set => this.SetProperty(ref this.привязкаViewColumns, value); }

        public TableViewKinds SelectedTableViewKind { get => this.selectedTableViewKind; set => this.SetProperty(ref this.selectedTableViewKind, value); }

        public TMPApplication.VisualTheme Theme { get => this.theme; set => this.SetProperty(ref this.theme, value); }

        public string LastUsedDataFileName { get => this.lastUsedDataFileName; set => this.SetProperty(ref this.lastUsedDataFileName, value); }

        public bool IsCellSelectionEnabled { get => this.isCellSelectionEnabled; set => this.SetProperty(ref this.isCellSelectionEnabled, value); }

        public uint NumberOfApartmentsInAnApartmentBuilding { get => this.numberOfApartmentsInAnApartmentBuilding; set => this.SetProperty(ref this.numberOfApartmentsInAnApartmentBuilding, value); }

        public static System.Globalization.CultureInfo CurrentCulture => System.Threading.Thread.CurrentThread.CurrentUICulture;

        #endregion

        #region Public methods

        public void Load()
        {
            this.FontSize = this.Settings.FontSize;

            this.AramisDBPath = this.Settings.AramisDBPath;
            this.DataFilesStorePath = this.Settings.DataFilesStorePath;

            this.ChangesOfMetersFields = this.Settings.ChangesOfMetersFields;
            this.ViewModelsTableColumns = this.Settings.ViewModelsTableColumns;
            this.SummaryInfoFields = this.Settings.SummaryInfoFields;

            this.SelectedSummaryView = this.Settings.SelectedSummaryView;
            this.SelectedTableViewKind = this.Settings.SelectedTableViewKind;

            this.BaseViewColumns = this.Settings.BaseViewColumns;
            this.ShortViewColumns = this.Settings.ShortViewColumns;
            this.DetailedViewColumns = this.Settings.DetailedViewColumns;
            this.ОплатаViewColumns = this.Settings.ОплатаViewColumns;
            this.ПривязкаViewColumns = this.Settings.ПривязкаViewColumns;

            this.Theme = this.Settings.Theme;

            this.Theme = this.Settings.Theme;
            this.LastUsedDataFileName = this.Settings.LastUsedDataFileName;
            this.IsCellSelectionEnabled = this.Settings.IsCellSelectionEnabled;
            this.NumberOfApartmentsInAnApartmentBuilding = this.Settings.NumberOfApartmentsInAnApartmentBuilding;
        }

        public void Save()
        {
            this.Settings.FontSize = this.FontSize;

            this.Settings.AramisDBPath = this.AramisDBPath;
            this.Settings.DataFilesStorePath = this.DataFilesStorePath;

            this.Settings.ChangesOfMetersFields = this.ChangesOfMetersFields;
            this.Settings.ViewModelsTableColumns = this.ViewModelsTableColumns;
            this.Settings.SummaryInfoFields = this.SummaryInfoFields;

            this.Settings.SelectedSummaryView = this.SelectedSummaryView;
            this.Settings.SelectedTableViewKind = this.SelectedTableViewKind;

            this.Settings.BaseViewColumns = this.BaseViewColumns;
            this.Settings.ShortViewColumns = this.ShortViewColumns;
            this.Settings.DetailedViewColumns = this.DetailedViewColumns;
            this.Settings.ОплатаViewColumns = this.ОплатаViewColumns;
            this.Settings.ПривязкаViewColumns = this.ПривязкаViewColumns;

            this.Settings.Theme = this.Theme;

            this.Settings.Theme = this.Theme;
            this.Settings.LastUsedDataFileName = this.LastUsedDataFileName;
            this.Settings.IsCellSelectionEnabled = this.IsCellSelectionEnabled;
            this.Settings.NumberOfApartmentsInAnApartmentBuilding = this.NumberOfApartmentsInAnApartmentBuilding;

            this.Settings.Save();
        }

        public static IEnumerable<PlusPropertyDescriptor> GetElectricitySupplyPropertyDescriptors()
        {
            var electricitySupplyFields = new ObservableCollection<string>(
                ModelHelper.ElectricitySupplyPropertiesCollection.Values.Select(i => i.Name).ToList());

            IEnumerable<PlusPropertyDescriptor> fields = ModelHelper.GetFields(ModelHelper.ElectricitySupplyPropertiesCollection, electricitySupplyFields);
            int index = 0;
            foreach (var item in fields)
            {
                item.Order = index++;
            }

            return fields.OrderBy(i => i.Order);
        }

        public IEnumerable<PlusPropertyDescriptor> GetChangesOfMetersPropertyDescriptors()
        {
            IEnumerable<PlusPropertyDescriptor> fields = ModelHelper.GetFields(ModelHelper.ChangesOfMetersPropertiesCollection, this.Settings.ChangesOfMetersFields);
            int index = 0;
            foreach (var item in fields)
            {
                item.Order = index++;
            }

            return fields.OrderBy(i => i.Order);
        }

        #endregion

        #region Private methods

        private static ICollection<PlusPropertyDescriptor> GetOnlyVisibleAndOrderedFields(ICollection<PlusPropertyDescriptor> props)
        {
            if (props == null)
            {
                return null;
            }

            return new List<PlusPropertyDescriptor>(props.Where(i => i.IsVisible).OrderBy(i => i.Order));
        }

        #endregion
    }
}
