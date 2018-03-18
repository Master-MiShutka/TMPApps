using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace TMP.UI.Controls.WPF.Reporting.MatrixGrid
{
    /// <summary>
    /// Панель для матрицы данных
    /// </summary>
    class MatrixGrid : Grid
    {
        #region Constructor

        public MatrixGrid()
        {
            _childToMonitorMap = new Dictionary<DependencyObject, MatrixGridChildMonitor>();
            _converter = new MatrixGridChildConverter(this);
        }

        #endregion // Constructor

        #region OnVisualChildrenChanged

        protected override void OnVisualChildrenChanged(DependencyObject visualAdded, DependencyObject visualRemoved)
        {
            base.OnVisualChildrenChanged(visualAdded, visualRemoved);

            if (visualAdded != null)
                this.StartMonitoringChildElement(visualAdded);
            else
                this.StopMonitoringChildElement(visualRemoved);
        }

        #endregion // OnVisualChildrenChanged

        #region Inspect Row/Column Index

        internal void InspectRowIndex(int index)
        {
            base.Dispatcher.BeginInvoke(new Action(delegate
                {
                    while (base.RowDefinitions.Count - 1 < index)
                    {
                        base.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Auto) });
                    }
                }));
        }

        internal void InspectColumnIndex(int index)
        {
            base.Dispatcher.BeginInvoke(new Action(delegate
                {
                    while (base.ColumnDefinitions.Count - 1 < index)
                    {
                        base.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Auto) });
                    }
                }));
        }

        #endregion // Inspect Row/Column Index

        #region Private Helpers

        Binding CreateMonitorBinding(DependencyObject childElement, DependencyProperty property)
        {
            return new Binding
            {
                Converter = _converter,
                ConverterParameter = property,
                Mode = BindingMode.OneWay,
                Path = new PropertyPath(property),
                Source = childElement
            };
        }

        void StartMonitoringChildElement(DependencyObject childElement)
        {
            // Создание объекта для отслеживания изменения прикрепленных свойств Grid.Row и Grid.Column у новых ячеек

            MatrixGridChildMonitor monitor = new MatrixGridChildMonitor();

            BindingOperations.SetBinding(
                monitor,
                MatrixGridChildMonitor.GridRowProperty,
                this.CreateMonitorBinding(childElement, Grid.RowProperty));

            BindingOperations.SetBinding(
                monitor,
                MatrixGridChildMonitor.GridColumnProperty,
                this.CreateMonitorBinding(childElement, Grid.ColumnProperty));

            _childToMonitorMap.Add(childElement, monitor);
        }

        void StopMonitoringChildElement(DependencyObject childElement)
        {
            // Удаление объекта для отслеживания изменения прикрепленных свойств Grid.Row и Grid.Column у новых ячеек

            if (_childToMonitorMap.ContainsKey(childElement))
            {
                MatrixGridChildMonitor monitor = _childToMonitorMap[childElement];
                BindingOperations.ClearAllBindings(monitor);
                _childToMonitorMap.Remove(childElement);
            }
        }

        #endregion // Private Helpers

        #region Fields

        readonly Dictionary<DependencyObject, MatrixGridChildMonitor> _childToMonitorMap;
        readonly MatrixGridChildConverter _converter;

        #endregion // Fields
    }
}
