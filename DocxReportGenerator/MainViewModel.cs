namespace TMP.Work.DocxReportGenerator
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using System.IO;
    using System.IO.Packaging;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using TemplateEngine.Docx;
    using TMP.Shared.Commands;
    using TMP.UI.Controls.WPF;

    public class MainViewModel : INotifyPropertyChanged
    {
        #region Fields

        private IEnumerable sourceTable = null;
        public string templateFileName = "Шаблон.docx";
        private string _resultFileName = "Отчёт";
        private string _resultFileNameFieldsDecription;
        private ReportCreationMode _selectedReportCreationMode = ReportCreationMode.One;
        private IEnumerable<Field> _listOfFieldsInTemplate = null;

        private Field _selectedField;

        #endregion

        #region Constructors

        public MainViewModel()
        {
            this.CommandPasteSourceFromClipboard = new DelegateCommand(() =>
            {
            });

            this.CommandClearSource = new DelegateCommand(() =>
            {
                this.SourceTable = null;
            }, (o) => this.SourceTable != null);

            this.CommandOpenTemplate = new DelegateCommand(() =>
            {
                System.Diagnostics.Process.Start(this.TemplateFileName);
            },
            (o) => File.Exists(this.templateFileName),
            "Открыть");
            this.CommandOpenReport = new DelegateCommand(() =>
            {
                System.Diagnostics.Process.Start(this.ResultFileName);
            },
            (o) => File.Exists(this._resultFileName),
            "Открыть");

            this.CommandClearReportStructure = new DelegateCommand(() =>
            {
                this.SourceTable = null;
                this.TemplateHasHierarchicalStructure = false;
                this.ListOfFieldsInTemplate = null;
            },
            (o) => true,
            "очистить всё");

            this.CommandParseTemplateStructure = new DelegateCommand(this.ParseTemplateStructure,
            (o) => true,
            "определить из шаблона");

            this.CommandStart = new DelegateCommand(this.Start,
            (o) => File.Exists(this._resultFileName),
            "Запуск");

            this.CommandCreateSourceWithFieldsFromTemplate = new DelegateCommand(this.CreateSourceWithFieldsFromTemplate,
                (o) => this.ListOfFieldsInTemplate != null && this.ListOfFieldsInTemplate.Count() > 0);

            // SourceTable = System.Linq.Enumerable.Range(0, 10).Select(o => new { A = o, B = "" });
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                return;
            }

            this.TemplateFileName = "template.docx";
        }

        #endregion

        #region Properties

        public IEnumerable<DataGridColumn> SourceTableColumns { get; private set; }

        public IEnumerable SourceTable
        {
            get => this.sourceTable;

            set
            {
                this.sourceTable = value;
                this.RaisePropertyChanged(nameof(this.SourceTable));
            }
        }

        public string TemplateFileName
        {
            get => this.templateFileName;

            set
            {
                this.CommandClearReportStructure.Execute(null);

                this.templateFileName = value;
                this.RaisePropertyChanged(nameof(this.TemplateFileName));
            }
        }

        public string ResultFileName
        {
            get => this._resultFileName;

            set
            {
                this._resultFileName = value;
                this.RaisePropertyChanged(nameof(this.ResultFileName));
            }
        }

        public string ResultFileNameFieldsDecription
        {
            get => this._resultFileNameFieldsDecription;

            private set
            {
                this._resultFileName = value;
                this.RaisePropertyChanged(nameof(this.ResultFileNameFieldsDecription));
            }
        }

        public ReportCreationMode SelectedReportCreationMode
        {
            get => this._selectedReportCreationMode;

            set
            {
                this._selectedReportCreationMode = value;
                this.RaisePropertyChanged(nameof(this.SelectedReportCreationMode));
                this.RaisePropertyChanged(nameof(this.ReportCreationModeDescription));
            }
        }

        public string ReportCreationModeDescription
        {
            get
            {
                if (this.SelectedReportCreationMode == ReportCreationMode.One)
                {
                    return "т.е. формируется один документ, в котором будет список или таблица,\nкаждый элемент которого(ой) будет содержать одно или\n несколько полей таблицы или списка";
                }
                else
                {
                    return "т.е. для каждой строки таблицы, заданной в первом пункте,\nформируется документ, в котором будут одно или\n несколько полей таблицы или списка";
                }
            }
        }

        public bool TemplateHasHierarchicalStructure { get; private set; }

        public ICommand CommandPasteSourceFromClipboard { get; private set; }

        public ICommand CommandClearSource { get; private set; }

        public ICommand CommandOpenTemplate { get; private set; }

        public ICommand CommandClearReportStructure { get; private set; }

        public ICommand CommandParseTemplateStructure { get; private set; }

        public ICommand CommandOpenReport { get; private set; }

        public ICommand CommandStart { get; private set; }

        public ICommand CommandCreateSourceWithFieldsFromTemplate { get; private set; }

        public IEnumerable<Field> ListOfFieldsInTemplate
        {
            get => this._listOfFieldsInTemplate == null ? null : this._listOfFieldsInTemplate.Where(f => f.HasChildren);

            private set
            {
                this._listOfFieldsInTemplate = value;
                this.RaisePropertyChanged(nameof(this.ListOfFieldsInTemplate));
            }
        }

        public Field SelectedField
        {
            get => this._selectedField;

            set
            {
                this._selectedField = value;
                this.RaisePropertyChanged(nameof(this.SelectedField));
            }
        }

        #endregion

        #region Public Methods

        #endregion

        #region Private Methods

        private void ParseTemplateStructure()
        {
            using (var tp = new TemplateProcessor(this.TemplateFileName))
            {
                this.ListOfFieldsInTemplate = tp.GetAllFields();
            }

            this.TemplateHasHierarchicalStructure = this.ListOfFieldsInTemplate != null && this.ListOfFieldsInTemplate.Count() > 0;
            this.RaisePropertyChanged(nameof(this.TemplateHasHierarchicalStructure));
        }

        private void CreateSourceWithFieldsFromTemplate()
        {
            if (this.SelectedReportCreationMode == ReportCreationMode.One)
            {
                this.SourceTableColumns = new List<DataGridTextColumn>(new DataGridTextColumn[]
                {
                    new DataGridTextColumn() { Header = "Название поля", Binding = new System.Windows.Data.Binding("FieldName") },
                    new DataGridTextColumn() { Header = "Значение поля", Binding = new System.Windows.Data.Binding("FieldValue") },
                });

                this.SourceTable = from field in this._listOfFieldsInTemplate
                                   where field.HasChildren == false
                                   orderby field.Name
                                   select new { FieldName = field.Name, FieldValue = string.Empty };
            }
            else
                if (this.SelectedReportCreationMode == ReportCreationMode.Multiple)
            {
                var list = this._listOfFieldsInTemplate
                           .Where(field => field.HasChildren == false)
                           .OrderBy(field => field.Name);

                this.SourceTableColumns = from field in list
                                          select new DataGridTextColumn() { Header = field.Name };

                var columns = from field in list
                              select new DataColumn(field.Name, typeof(string));

                DataTable table = new DataTable("source");
                table.Columns.AddRange(columns.ToArray());
                table.NewRow();
                this.SourceTable = table.AsEnumerable();
            }
        }

        private void Start()
        {
        }

        #endregion

        #region INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;

        protected bool SetProperty<T>(ref T field, T value, string propertyName = null)
        {
            if (Equals(field, value))
            {
                return false;
            }

            field = value;
            this.RaisePropertyChanged(propertyName);
            return true;
        }

        protected void RaisePropertyChanged(string propertyName = null)
        {
            this.OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            this.PropertyChanged?.Invoke(this, e);
        }
        #endregion
    }
}
