using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Input;
using System.Linq;
using System.IO;
using System.IO.Packaging;
using TemplateEngine.Docx;
using System.Data;
using TMP.UI.Controls.WPF;

namespace TMP.Work.DocxReportGenerator
{
    public class MainViewModel : INotifyPropertyChanged
    {
        #region Fields

        private IEnumerable _sourceTable = null;
        public string _templateFileName = "Шаблон.docx";
        private string _resultFileName = "Отчёт";
        private string _resultFileNameFieldsDecription;
        private ReportCreationMode _selectedReportCreationMode = ReportCreationMode.One;
        private IEnumerable<Field> _listOfFieldsInTemplate = null;

        private Field _selectedField;

        #endregion

        #region Constructors

        public MainViewModel()
        {
            CommandPasteSourceFromClipboard = new DelegateCommand(() =>
            {
                ;
            });

            CommandClearSource = new DelegateCommand(() =>
            {
                SourceTable = null;
            }, () => SourceTable != null);

            CommandOpenTemplate = new DelegateCommand(() =>
            {
                System.Diagnostics.Process.Start(TemplateFileName);
            },
            () => File.Exists(_templateFileName),
            "Открыть");
            CommandOpenReport = new DelegateCommand(() =>
            {
                System.Diagnostics.Process.Start(ResultFileName);
            },
            () => File.Exists(_resultFileName),
            "Открыть");

            CommandClearReportStructure = new DelegateCommand(() =>
            {
                SourceTable = null;
                TemplateHasHierarchicalStructure = false;
                ListOfFieldsInTemplate = null;
            },
            () => true,
            "очистить всё");

            CommandParseTemplateStructure = new DelegateCommand(ParseTemplateStructure,
            () => true,
            "определить из шаблона");

            CommandStart = new DelegateCommand(Start,
            () => File.Exists(_resultFileName),
            "Запуск");

            CommandCreateSourceWithFieldsFromTemplate = new DelegateCommand(CreateSourceWithFieldsFromTemplate,
                () => ListOfFieldsInTemplate != null && ListOfFieldsInTemplate.Count() > 0);

            //SourceTable = System.Linq.Enumerable.Range(0, 10).Select(o => new { A = o, B = "" });

            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                return;
            }

            TemplateFileName = "template.docx";
        }

        #endregion


        #region Properties

        

        public IEnumerable SourceTable
        {
            get { return _sourceTable; }
            set { _sourceTable = value; RaisePropertyChanged("SourceTable"); }
        }
        public string TemplateFileName
        {
            get { return _templateFileName; }
            set
            {
                CommandClearReportStructure.Execute(null);

                _templateFileName = value;
                RaisePropertyChanged("TemplateFileName");
            }
        }
        public string ResultFileName
        {
            get { return _resultFileName; }
            set
            {
                _resultFileName = value;
                RaisePropertyChanged("ResultFileName");
            }
        }
        public string ResultFileNameFieldsDecription
        {
            get { return _resultFileNameFieldsDecription; }
            private set { _resultFileName = value; RaisePropertyChanged("ResultFileNameFieldsDecription"); }
        }
        public ReportCreationMode SelectedReportCreationMode
        {
            get { return _selectedReportCreationMode; }
            set
            {
                _selectedReportCreationMode = value;
                RaisePropertyChanged("SelectedReportCreationMode");
                RaisePropertyChanged("ReportCreationModeDescription");
            }
        }

        public string ReportCreationModeDescription
        {
            get
            {
                if (SelectedReportCreationMode == ReportCreationMode.One)
                    return "т.е. формируется один документ, в котором будет список или таблица,\nкаждый элемент которого(ой) будет содержать одно или\n несколько полей таблицы или списка";
                else
                    return "т.е. для каждой строки таблицы, заданной в первом пункте,\nформируется документ, в котором будут одно или\n несколько полей таблицы или списка";
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
            get { return _listOfFieldsInTemplate == null ? null : _listOfFieldsInTemplate.Where(f => f.HasChildren); }
            private set { _listOfFieldsInTemplate = value; RaisePropertyChanged("ListOfFieldsInTemplate"); }
        }
        public Field SelectedField
        {
            get { return _selectedField; }
            set { _selectedField = value; RaisePropertyChanged("SelectedField"); }
        }

        #endregion

        #region Public Methods


        #endregion

        #region Private Methods

        private void ParseTemplateStructure()
        {
            using (var tp = new TemplateProcessor(TemplateFileName))
            {
                ListOfFieldsInTemplate = tp.GetAllFields();
            }
            TemplateHasHierarchicalStructure = ListOfFieldsInTemplate != null && ListOfFieldsInTemplate.Count() > 0;
            RaisePropertyChanged("TemplateHasHierarchicalStructure");
        }

        private void CreateSourceWithFieldsFromTemplate()
        {
            if (SelectedReportCreationMode == ReportCreationMode.One)
            {
                SourceTable = from field in _listOfFieldsInTemplate
                              where field.HasChildren == false
                              orderby field.Name
                              select new { FieldName = field.Name, FieldValue = String.Empty };
            }
            else
                if (SelectedReportCreationMode == ReportCreationMode.Multiple)
            {
                var columns = from field in _listOfFieldsInTemplate
                              where field.HasChildren == false
                              orderby field.Name
                              select new DataColumn(field.Name, typeof(string));

                DataTable table = new DataTable("source");
                table.Columns.AddRange(columns.ToArray());
                SourceTable = table.AsEnumerable();
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
            if (Equals(field, value)) { return false; }

            field = value;
            RaisePropertyChanged(propertyName);
            return true;
        }
        protected void RaisePropertyChanged(string propertyName = null)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)

        {
            PropertyChanged?.Invoke(this, e);
        }
        #endregion
    }
}
