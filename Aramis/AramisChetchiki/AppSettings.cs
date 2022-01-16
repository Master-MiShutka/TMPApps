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

        private readonly PlusPropertyDescriptorsCollection defaultSummaryInfoFields;
        private readonly PlusPropertyDescriptorsCollection defaultChangesOfMetersFields;
        private readonly PlusPropertyDescriptorsCollection defaultBaseViewColumns;
        private readonly PlusPropertyDescriptorsCollection defaultShortViewColumns;
        private readonly PlusPropertyDescriptorsCollection defaultDetailedViewColumns;
        private readonly PlusPropertyDescriptorsCollection defaultОплатаViewColumns;
        private readonly PlusPropertyDescriptorsCollection defaultПривязкаViewColumns;

        private double fontSize = 14.0;
        private string aramisDBPath = "d:\\aramis\\Disks\\OSHM\\aramis";
        private string dataFilesStorePath = string.Empty;

        private PlusPropertyDescriptorsCollection changesOfMetersFields;
        private PlusPropertyDescriptorsCollection summaryInfoFields;
        private PlusPropertyDescriptorsCollection baseViewColumns;
        private PlusPropertyDescriptorsCollection shortViewColumns;
        private PlusPropertyDescriptorsCollection detailedViewColumns;
        private PlusPropertyDescriptorsCollection оплатаViewColumns;
        private PlusPropertyDescriptorsCollection привязкаViewColumns;

        private Dictionary<ViewModel.IViewModel, List<DataGridWpf.DataGridWpfColumnViewModel>> viewModelsTableColumns;
        private InfoViewType selectedSummaryView = InfoViewType.ViewAsDiagram;
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

            this.defaultSummaryInfoFields = new PlusPropertyDescriptorsCollection(ModelHelper.MeterSummaryInfoItemDescriptors);
            System.Diagnostics.Debug.Assert(this.defaultSummaryInfoFields.Count > 0, "DefaultSummaryInfoFields.Count <= 0");

            this.defaultChangesOfMetersFields = new PlusPropertyDescriptorsCollection(ModelHelper.ChangesOfMetersDescriptors);
            if (this.defaultChangesOfMetersFields.Count == 0)
            {
                System.Diagnostics.Debugger.Break();
            }

            this.Load();

            this.ПолныйViewColumns = Meter.GetSetOfColumns("ПолныйView").ToPlusPropertyDescriptorsCollection();

            this.defaultBaseViewColumns = Meter.GetSetOfColumns("BaseView").ToPlusPropertyDescriptorsCollection();
            this.defaultShortViewColumns = Meter.GetSetOfColumns("ShortView").ToPlusPropertyDescriptorsCollection();
            this.defaultDetailedViewColumns = Meter.GetSetOfColumns("DetailedView").ToPlusPropertyDescriptorsCollection();
            this.defaultОплатаViewColumns = Meter.GetSetOfColumns("ОплатаView").ToPlusPropertyDescriptorsCollection();
            this.defaultПривязкаViewColumns = Meter.GetSetOfColumns("ПривязкаView").ToPlusPropertyDescriptorsCollection();

            if (this.SummaryInfoFields == null || this.SummaryInfoFields.Count == 0)
            {
                this.SummaryInfoFields = this.defaultSummaryInfoFields;
            }

            if (this.ChangesOfMetersFields == null || this.ChangesOfMetersFields.Count == 0)
            {
                this.ChangesOfMetersFields = this.defaultChangesOfMetersFields;
            }

            if (this.BaseViewColumns == null || this.BaseViewColumns.Count == 0)
            {
                this.BaseViewColumns = this.defaultBaseViewColumns;
            }

            if (this.ShortViewColumns == null || this.ShortViewColumns.Count == 0)
            {
                this.ShortViewColumns = this.defaultShortViewColumns;
            }

            if (this.DetailedViewColumns == null || this.DetailedViewColumns.Count == 0)
            {
                this.DetailedViewColumns = this.defaultDetailedViewColumns;
            }

            if (this.ОплатаViewColumns == null || this.ОплатаViewColumns.Count == 0)
            {
                this.ОплатаViewColumns = this.defaultОплатаViewColumns;
            }

            if (this.ПривязкаViewColumns == null || this.ПривязкаViewColumns.Count == 0)
            {
                this.ПривязкаViewColumns = this.defaultПривязкаViewColumns;
            }

            if (this.ViewModelsTableColumns == null)
            {
                this.ViewModelsTableColumns = new Dictionary<ViewModel.IViewModel, List<DataGridWpf.DataGridWpfColumnViewModel>>();
            }
        }

        #region Public properties

        public static readonly StringComparison StringComparisonMethod = StringComparison.Ordinal;

        public double FontSize { get => this.fontSize; set => this.SetProperty(ref this.fontSize, value); }

        public string AramisDBPath { get => this.aramisDBPath; set => this.SetProperty(ref this.aramisDBPath, value); }

        public string DataFilesStorePath { get => this.dataFilesStorePath; set => this.SetProperty(ref this.dataFilesStorePath, value); }

        public PlusPropertyDescriptorsCollection ChangesOfMetersFields { get => this.changesOfMetersFields; set => this.SetProperty(ref this.changesOfMetersFields, value); }

        public Dictionary<ViewModel.IViewModel, List<DataGridWpf.DataGridWpfColumnViewModel>> ViewModelsTableColumns { get => this.viewModelsTableColumns; set => this.SetProperty(ref this.viewModelsTableColumns, value); }

        public PlusPropertyDescriptorsCollection SummaryInfoFields { get => this.summaryInfoFields; set => this.SetProperty(ref this.summaryInfoFields, value); }

        public InfoViewType SelectedSummaryView { get => this.selectedSummaryView; set => this.SetProperty(ref this.selectedSummaryView, value); }

        public PlusPropertyDescriptorsCollection ПолныйViewColumns { get; }

        public PlusPropertyDescriptorsCollection BaseViewColumns { get => this.baseViewColumns; set => this.SetProperty(ref this.baseViewColumns, value); }

        public PlusPropertyDescriptorsCollection ShortViewColumns { get => this.shortViewColumns; set => this.SetProperty(ref this.shortViewColumns, value); }

        public PlusPropertyDescriptorsCollection DetailedViewColumns { get => this.detailedViewColumns; set => this.SetProperty(ref this.detailedViewColumns, value); }

        public PlusPropertyDescriptorsCollection ОплатаViewColumns { get => this.оплатаViewColumns; set => this.SetProperty(ref this.оплатаViewColumns, value); }

        public PlusPropertyDescriptorsCollection ПривязкаViewColumns { get => this.привязкаViewColumns; set => this.SetProperty(ref this.привязкаViewColumns, value); }

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

            this.ChangesOfMetersFields = this.Settings.ChangesOfMetersFields.ToPlusPropertyDescriptorsCollection();
            this.SummaryInfoFields = this.Settings.SummaryInfoFields.ToPlusPropertyDescriptorsCollection();
            this.BaseViewColumns = this.Settings.BaseViewColumns.ToPlusPropertyDescriptorsCollection();
            this.ShortViewColumns = this.Settings.ShortViewColumns.ToPlusPropertyDescriptorsCollection();
            this.DetailedViewColumns = this.Settings.DetailedViewColumns.ToPlusPropertyDescriptorsCollection();
            this.ОплатаViewColumns = this.Settings.ОплатаViewColumns.ToPlusPropertyDescriptorsCollection();
            this.ПривязкаViewColumns = this.Settings.ПривязкаViewColumns.ToPlusPropertyDescriptorsCollection();

            this.ViewModelsTableColumns = this.Settings.ViewModelsTableColumns;

            this.SelectedSummaryView = this.Settings.SelectedSummaryView;
            this.SelectedTableViewKind = this.Settings.SelectedTableViewKind;

            this.Theme = this.Settings.Theme;

            this.Theme = this.Settings.Theme;
            this.LastUsedDataFileName = this.Settings.LastUsedDataFileName;
            this.IsCellSelectionEnabled = this.Settings.IsCellSelectionEnabled;
            this.NumberOfApartmentsInAnApartmentBuilding = this.Settings.NumberOfApartmentsInAnApartmentBuilding;
        }

        public void Save()
        {
            var dialog = (App.Current as TMPApplication.TMPApp).MainWindowWithDialogs.DialogWaitingScreen("сохранение настроек ...", indeterminate: true);
            dialog.Show();

            this.Settings.FontSize = this.FontSize;

            this.Settings.AramisDBPath = this.AramisDBPath;
            this.Settings.DataFilesStorePath = this.DataFilesStorePath;

            this.Settings.ChangesOfMetersFields = new ChangesOfMetersFieldsCollection(this.ChangesOfMetersFields);
            this.Settings.SummaryInfoFields = new SummaryInfoFieldsCollection(this.SummaryInfoFields);
            this.Settings.BaseViewColumns = new MeterFieldsCollection(this.BaseViewColumns);
            this.Settings.ShortViewColumns = new MeterFieldsCollection(this.ShortViewColumns);
            this.Settings.DetailedViewColumns = new MeterFieldsCollection(this.DetailedViewColumns);
            this.Settings.ОплатаViewColumns = new MeterFieldsCollection(this.ОплатаViewColumns);
            this.Settings.ПривязкаViewColumns = new MeterFieldsCollection(this.ПривязкаViewColumns);

            this.Settings.ViewModelsTableColumns = this.ViewModelsTableColumns;

            this.Settings.SelectedSummaryView = this.SelectedSummaryView;
            this.Settings.SelectedTableViewKind = this.SelectedTableViewKind;

            this.Settings.Theme = this.Theme;

            this.Settings.Theme = this.Theme;
            this.Settings.LastUsedDataFileName = this.LastUsedDataFileName;
            this.Settings.IsCellSelectionEnabled = this.IsCellSelectionEnabled;
            this.Settings.NumberOfApartmentsInAnApartmentBuilding = this.NumberOfApartmentsInAnApartmentBuilding;

            this.Settings.Save();

            dialog.Close();
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

        public MeterFieldsCollection GetMeterFieldsCollectionByTableViewKind(TableViewKinds tableViewKind)
        {
            return tableViewKind switch
            {
                TableViewKinds.BaseView => new MeterFieldsCollection(this.BaseViewColumns),
                TableViewKinds.DetailedView => new MeterFieldsCollection(this.DetailedViewColumns),
                TableViewKinds.ShortView => new MeterFieldsCollection(this.ShortViewColumns),
                TableViewKinds.ОплатаView => new MeterFieldsCollection(this.ОплатаViewColumns),
                TableViewKinds.ПривязкаView => new MeterFieldsCollection(this.ПривязкаViewColumns),
                TableViewKinds.ПолныйView => new MeterFieldsCollection(this.ПолныйViewColumns),
                _ => throw new NotImplementedException("Unknown TableView"),
            };
        }

        #endregion
    }
}
