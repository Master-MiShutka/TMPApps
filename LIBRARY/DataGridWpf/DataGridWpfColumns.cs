#region (c) 2019 Gilles Macabies

// Author     : Gilles Macabies
// Solution   : DataGridFilter
// Projet     : DataGridFilter
// File       : DataGridTextColumn.cs
// Created    : 09/11/2019
#endregion (c) 2019 Gilles Macabies

using System;
using System.Windows;
using System.Windows.Data;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable CheckNamespace
namespace DataGridWpf
{
    public interface IDataGridWpfColumn
    {
        string FieldName { get; set; }

        bool IsColumnFiltered { get; set; }
    }

    public sealed class WpfDataGridTemplateColumn : System.Windows.Controls.DataGridTemplateColumn, IDataGridWpfColumn
    {
        internal WpfDataGridTemplateColumn()
        {
        }

        #region Public DependencyProperties

        /// <summary>
        /// FieldName Dependency Property.
        /// </summary>
        public static readonly DependencyProperty FieldNameProperty =
            DependencyProperty.Register(nameof(FieldName), typeof(string), typeof(WpfDataGridTemplateColumn),
                new PropertyMetadata(default));

        /// <summary>
        /// IsColumnFiltered Dependency Property.
        /// </summary>
        public static readonly DependencyProperty IsColumnFilteredProperty =
                    DependencyProperty.Register(nameof(IsColumnFiltered), typeof(bool), typeof(WpfDataGridTemplateColumn),
                new PropertyMetadata(true));

        #endregion Public DependencyProperties

        #region Public Properties

        public string FieldName
        {
            get => (string)this.GetValue(FieldNameProperty);
            set => this.SetValue(FieldNameProperty, value);
        }

        public bool IsColumnFiltered
        {
            get => (bool)this.GetValue(IsColumnFilteredProperty);
            set => this.SetValue(IsColumnFilteredProperty, value);
        }

        #endregion Public Properties

        public override object OnCopyingCellClipboardContent(object item)
        {
            var value = base.OnCopyingCellClipboardContent(item);

            return value;
        }
    }

    public sealed class WpfDataGridComboBoxColumn : System.Windows.Controls.DataGridComboBoxColumn, IDataGridWpfColumn
    {
        internal WpfDataGridComboBoxColumn()
        {
        }

        #region Public DependencyProperties

        /// <summary>
        /// FieldName Dependency Property.
        /// </summary>
        public static readonly DependencyProperty FieldNameProperty =
            DependencyProperty.Register(nameof(FieldName), typeof(string), typeof(WpfDataGridComboBoxColumn),
                new PropertyMetadata(default));

        /// <summary>
        /// IsColumnFiltered Dependency Property.
        /// </summary>
        public static readonly DependencyProperty IsColumnFilteredProperty =
                    DependencyProperty.Register(nameof(IsColumnFiltered), typeof(bool), typeof(WpfDataGridComboBoxColumn),
                new PropertyMetadata(true));

        #endregion Public DependencyProperties

        #region Public Properties

        public string FieldName
        {
            get => (string)this.GetValue(FieldNameProperty);
            set => this.SetValue(FieldNameProperty, value);
        }

        public bool IsColumnFiltered
        {
            get => (bool)this.GetValue(IsColumnFilteredProperty);
            set => this.SetValue(IsColumnFilteredProperty, value);
        }

        #endregion Public Properties

        public override object OnCopyingCellClipboardContent(object item)
        {
            var value = base.OnCopyingCellClipboardContent(item);

            return value;
        }
    }

    public sealed class WpfDataGridTextColumn : System.Windows.Controls.DataGridTextColumn, IDataGridWpfColumn
    {
        internal WpfDataGridTextColumn()
        {
        }

        #region Public DependencyProperties

        /// <summary>
        /// FieldName Dependency Property.
        /// </summary>
        public static readonly DependencyProperty FieldNameProperty =
            DependencyProperty.Register(nameof(FieldName), typeof(string), typeof(WpfDataGridTextColumn),
                new PropertyMetadata(default));

        /// <summary>
        /// IsColumnFiltered Dependency Property.
        /// </summary>
        public static readonly DependencyProperty IsColumnFilteredProperty =
                    DependencyProperty.Register(nameof(IsColumnFiltered), typeof(bool), typeof(WpfDataGridTextColumn),
                new PropertyMetadata(true));

        #endregion Public DependencyProperties

        #region Public Properties

        public string FieldName
        {
            get => (string)this.GetValue(FieldNameProperty);
            set => this.SetValue(FieldNameProperty, value);
        }

        public bool IsColumnFiltered
        {
            get => (bool)this.GetValue(IsColumnFilteredProperty);
            set => this.SetValue(IsColumnFilteredProperty, value);
        }

        #endregion Public Properties

        public override object OnCopyingCellClipboardContent(object item)
        {
            var value = base.OnCopyingCellClipboardContent(item);

            var dataFormat = this.Binding?.StringFormat;

            if (string.IsNullOrWhiteSpace(dataFormat) == false)
            {
                return string.Format(dataFormat, value);
            }
            else
            {
                return value;
            }
        }
    }

    public sealed class WpfDataGridCheckBoxColumn : System.Windows.Controls.DataGridTemplateColumn, IDataGridWpfColumn
    {
        internal WpfDataGridCheckBoxColumn()
        {
        }

        #region Public DependencyProperties

        /// <summary>
        /// IsColumnFiltered Dependency Property.
        /// </summary>
        public static readonly DependencyProperty IsColumnFilteredProperty =
                    DependencyProperty.Register(nameof(IsColumnFiltered), typeof(bool), typeof(WpfDataGridCheckBoxColumn),
                new PropertyMetadata(true));

        /// <summary>
        /// FieldName Dependency Property.
        /// </summary>
        public static readonly DependencyProperty FieldNameProperty =
            DependencyProperty.Register(nameof(FieldName), typeof(string), typeof(WpfDataGridCheckBoxColumn),
                new PropertyMetadata(default));

        #endregion Public DependencyProperties

        #region Public Properties

        public string FieldName
        {
            get => (string)this.GetValue(FieldNameProperty);
            set
            {
                this.SetValue(FieldNameProperty, value);
                this.CellTemplate = this.CreateCellTemplate();
            }
        }

        public bool IsColumnFiltered
        {
            get => (bool)this.GetValue(IsColumnFilteredProperty);
            set => this.SetValue(IsColumnFilteredProperty, value);
        }

        #endregion Public Properties

        public override object OnCopyingCellClipboardContent(object item)
        {
            var value = base.OnCopyingCellClipboardContent(item);

            if (value is bool boolValue)
            {
                return boolValue ? "да" : "нет";
            }
            else
            {
                return value;
            }
        }

        private DataTemplate CreateCellTemplate()
        {
            // (Type viewModelType, Type viewType)
            const string xamlTemplate = "<DataTemplate><TextBlock x:Name=\"txt\" TextAlignment=\"Center\"/></DataTemplate>"; // "<DataTemplate DataType=\"{{x:Type vm:{0}}}\"><v:{1} /></DataTemplate>";
            var xaml = string.Format(xamlTemplate); // , viewModelType.Name, viewType.Name, viewModelType.Namespace, viewType.Namespace);

            var context = new System.Windows.Markup.ParserContext();

            context.XamlTypeMapper = new System.Windows.Markup.XamlTypeMapper(new string[0]);

            // context.XamlTypeMapper.AddMappingProcessingInstruction("vm", viewModelType.Namespace, viewModelType.Assembly.FullName);
            // context.XamlTypeMapper.AddMappingProcessingInstruction("v", viewType.Namespace, viewType.Assembly.FullName);
            context.XmlnsDictionary.Add(string.Empty, "http://schemas.microsoft.com/winfx/2006/xaml/presentation");
            context.XmlnsDictionary.Add("x", "http://schemas.microsoft.com/winfx/2006/xaml");

            // context.XmlnsDictionary.Add("vm", "vm");
            // context.XmlnsDictionary.Add("v", "v");
            var template = (DataTemplate)System.Windows.Markup.XamlReader.Parse(xaml, context);

            DataTrigger dataTrigger = new DataTrigger
            {
                Binding = new Binding(this.FieldName),
                Value = true,
            };
            dataTrigger.Setters.Add(new Setter(System.Windows.Controls.TextBlock.TextProperty, "✓", "txt"));
            template.Triggers.Add(dataTrigger);

            dataTrigger = new DataTrigger
            {
                Binding = new Binding(this.FieldName),
                Value = true,
            };
            dataTrigger.Setters.Add(new Setter(System.Windows.Controls.TextBlock.ToolTipProperty, "да", "txt"));
            template.Triggers.Add(dataTrigger);

            dataTrigger = new DataTrigger
            {
                Binding = new Binding(this.FieldName),
                Value = false,
            };
            dataTrigger.Setters.Add(new Setter(System.Windows.Controls.TextBlock.ToolTipProperty, "нет", "txt"));
            template.Triggers.Add(dataTrigger);

            return template;
        }
    }
}