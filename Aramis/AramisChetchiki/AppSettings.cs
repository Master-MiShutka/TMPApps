namespace TMP.WORK.AramisChetchiki
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
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

        private bool inductiveMeterIsDefaultUnTrusted = true;
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

        private TMP.Shared.SerializableDictionary<string, string> viewModelsTableColumns;
        private InfoViewType selectedSummaryView = InfoViewType.ViewAsDiagram;
        private TableViewKinds selectedTableViewKind = TableViewKinds.BaseView;
        private TMPApplication.VisualTheme theme;
        private string lastUsedDataFileName;
        private bool isCellSelectionEnabled = false;
        private uint numberOfApartmentsInAnApartmentBuilding = 16;

        private Properties.Settings Settings => Properties.Settings.Default;

        #endregion

        private static AppSettings defaultInstance = new AppSettings();

        public static AppSettings Default => defaultInstance;

        public AppSettings()
        {
            this.viewModelsTableColumns = new TMP.Shared.SerializableDictionary<string, string>();

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
                this.summaryInfoFields = this.defaultSummaryInfoFields;
            }

            if (this.ChangesOfMetersFields == null || this.ChangesOfMetersFields.Count == 0)
            {
                this.changesOfMetersFields = this.defaultChangesOfMetersFields;
            }

            if (this.BaseViewColumns == null || this.BaseViewColumns.Count == 0)
            {
                this.baseViewColumns = this.defaultBaseViewColumns;
            }

            if (this.ShortViewColumns == null || this.ShortViewColumns.Count == 0)
            {
                this.shortViewColumns = this.defaultShortViewColumns;
            }

            if (this.DetailedViewColumns == null || this.DetailedViewColumns.Count == 0)
            {
                this.detailedViewColumns = this.defaultDetailedViewColumns;
            }

            if (this.ОплатаViewColumns == null || this.ОплатаViewColumns.Count == 0)
            {
                this.оплатаViewColumns = this.defaultОплатаViewColumns;
            }

            if (this.ПривязкаViewColumns == null || this.ПривязкаViewColumns.Count == 0)
            {
                this.привязкаViewColumns = this.defaultПривязкаViewColumns;
            }

            if (this.ViewModelsTableColumns == null)
            {
                this.viewModelsTableColumns = new TMP.Shared.SerializableDictionary<string, string>();
            }
        }

        #region Public properties

        public static readonly StringComparison StringComparisonMethod = StringComparison.Ordinal;

        public bool InductiveMeterIsDefaultUnTrusted { get => this.inductiveMeterIsDefaultUnTrusted; set => this.SetProperty(ref this.inductiveMeterIsDefaultUnTrusted, value); }

        public double FontSize { get => this.fontSize; set => this.SetProperty(ref this.fontSize, value); }

        public string AramisDBPath { get => this.aramisDBPath; set => this.SetProperty(ref this.aramisDBPath, value); }

        public string DataFilesStorePath { get => this.dataFilesStorePath; set => this.SetProperty(ref this.dataFilesStorePath, value); }

        public PlusPropertyDescriptorsCollection ChangesOfMetersFields { get => this.changesOfMetersFields; set => this.SetProperty(ref this.changesOfMetersFields, value); }

        public TMP.Shared.SerializableDictionary<string, string> ViewModelsTableColumns { get => this.viewModelsTableColumns; set => this.SetProperty(ref this.viewModelsTableColumns, value); }

        public PlusPropertyDescriptorsCollection SummaryInfoFields { get => this.summaryInfoFields; set => this.SetProperty(ref this.summaryInfoFields, value); }

        public InfoViewType SelectedSummaryView { get => this.selectedSummaryView; set => this.SetProperty(ref this.selectedSummaryView, value); }

        public PlusPropertyDescriptorsCollection ПолныйViewColumns { get; }

        public PlusPropertyDescriptorsCollection BaseViewColumns { get => this.baseViewColumns; set => this.SetProperty(ref this.baseViewColumns, value); }

        public PlusPropertyDescriptorsCollection ShortViewColumns { get => this.shortViewColumns; set => this.SetProperty(ref this.shortViewColumns, value); }

        public PlusPropertyDescriptorsCollection DetailedViewColumns { get => this.detailedViewColumns; set => this.SetProperty(ref this.detailedViewColumns, value); }

        public PlusPropertyDescriptorsCollection ОплатаViewColumns { get => this.оплатаViewColumns; set => this.SetProperty(ref this.оплатаViewColumns, value); }

        public PlusPropertyDescriptorsCollection ПривязкаViewColumns { get => this.привязкаViewColumns; set => this.SetProperty(ref this.привязкаViewColumns, value); }

        public TableViewKinds SelectedTableViewKind { get => this.selectedTableViewKind; set => this.SetProperty(ref this.selectedTableViewKind, value); }

        public string LastUsedDataFileName { get => this.lastUsedDataFileName; set => this.SetProperty(ref this.lastUsedDataFileName, value); }

        public bool IsCellSelectionEnabled { get => this.isCellSelectionEnabled; set => this.SetProperty(ref this.isCellSelectionEnabled, value); }

        public uint NumberOfApartmentsInAnApartmentBuilding { get => this.numberOfApartmentsInAnApartmentBuilding; set => this.SetProperty(ref this.numberOfApartmentsInAnApartmentBuilding, value); }

        public static System.Globalization.CultureInfo CurrentCulture => System.Threading.Thread.CurrentThread.CurrentUICulture;

        #endregion

        #region Public methods

        public void Load()
        {
            this.InductiveMeterIsDefaultUnTrusted = this.Settings.InductiveMeterIsDefaultUnTrusted;
            this.FontSize = this.Settings.FontSize;

            this.AramisDBPath = this.Settings.AramisDBPath;
            this.DataFilesStorePath = this.Settings.DataFilesStorePath;

            PlusPropertyDescriptorsCollection getCollection(BaseFieldsCollection bfc)
            {
                if (bfc == null)
                {
                    return null;
                }

                return bfc.ToPlusPropertyDescriptorsCollection();
            }

            this.ChangesOfMetersFields = getCollection(this.Settings.ChangesOfMetersFields);
            this.SummaryInfoFields = getCollection(this.Settings.SummaryInfoFields);
            this.BaseViewColumns = getCollection(this.Settings.BaseViewColumns);
            this.ShortViewColumns = getCollection(this.Settings.ShortViewColumns);
            this.DetailedViewColumns = getCollection(this.Settings.DetailedViewColumns);
            this.ОплатаViewColumns = getCollection(this.Settings.ОплатаViewColumns);
            this.ПривязкаViewColumns = getCollection(this.Settings.ПривязкаViewColumns);

            this.ViewModelsTableColumns = this.Settings.ViewModelsTableColumns;

            this.SelectedSummaryView = this.Settings.SelectedSummaryView;
            this.SelectedTableViewKind = this.Settings.SelectedTableViewKind;

            App.Instance.SelectedVisualTheme = this.Settings.Theme;

            this.LastUsedDataFileName = this.Settings.LastUsedDataFileName;
            this.IsCellSelectionEnabled = this.Settings.IsCellSelectionEnabled;
            this.NumberOfApartmentsInAnApartmentBuilding = this.Settings.NumberOfApartmentsInAnApartmentBuilding;
        }

        public void Save()
        {
            this.Settings.InductiveMeterIsDefaultUnTrusted = this.InductiveMeterIsDefaultUnTrusted;
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

            this.Settings.Theme = App.Instance.SelectedVisualTheme;

            this.Settings.LastUsedDataFileName = this.LastUsedDataFileName;
            this.Settings.IsCellSelectionEnabled = this.IsCellSelectionEnabled;
            this.Settings.NumberOfApartmentsInAnApartmentBuilding = this.NumberOfApartmentsInAnApartmentBuilding;

            this.Settings.Save();
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
