namespace TMP.UI.Controls.WPF.Reporting.MatrixGrid
{
    using System;
    using System.Collections.Generic;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Threading;

    /// <summary>
    /// Панель для матрицы данных
    /// </summary>
    internal class MatrixGrid : Grid
    {
        #region Constructor

        public MatrixGrid()
        {
            this.childToMonitorMap = new Dictionary<DependencyObject, MatrixGridChildMonitor>();
            this.converter = new MatrixGridChildConverter(this);
        }

        #endregion // Constructor

        #region OnVisualChildrenChanged

        protected override void OnVisualChildrenChanged(DependencyObject visualAdded, DependencyObject visualRemoved)
        {
            base.OnVisualChildrenChanged(visualAdded, visualRemoved);

            if (visualAdded != null)
            {
                this.StartMonitoringChildElement(visualAdded);
            }
            else
            {
                this.StopMonitoringChildElement(visualRemoved);
            }
        }

        #endregion // OnVisualChildrenChanged

        #region Inspect Row/Column Index

        internal void InspectRowIndex(int index)
        {
            DispatcherOperation o = base.Dispatcher.BeginInvoke(new Action(delegate
                {
                    while (base.RowDefinitions.Count - 1 < index)
                    {
                        base.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Auto) });
                    }
                }));
            var s = o.Status;
        }

        internal void InspectColumnIndex(int index)
        {
            DispatcherOperation o = base.Dispatcher.BeginInvoke(new Action(delegate
                 {
                     while (base.ColumnDefinitions.Count - 1 < index)
                     {
                         base.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Auto) });
                     }
                 }));
            var s = o.Status;
        }

        #endregion // Inspect Row/Column Index

        #region Private Helpers

        private Binding CreateMonitorBinding(DependencyObject childElement, DependencyProperty property)
        {
            return new Binding
            {
                Converter = this.converter,
                ConverterParameter = property,
                Mode = BindingMode.OneWay,
                Path = new PropertyPath(property),
                Source = childElement,
            };
        }

        private void StartMonitoringChildElement(DependencyObject childElement)
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

            this.childToMonitorMap.Add(childElement, monitor);
        }

        private void StopMonitoringChildElement(DependencyObject childElement)
        {
            // Удаление объекта для отслеживания изменения прикрепленных свойств Grid.Row и Grid.Column у новых ячеек
            if (this.childToMonitorMap.ContainsKey(childElement))
            {
                MatrixGridChildMonitor monitor = this.childToMonitorMap[childElement];
                BindingOperations.ClearAllBindings(monitor);
                this.childToMonitorMap.Remove(childElement);
            }
        }

        #endregion // Private Helpers

        #region Fields

        private readonly Dictionary<DependencyObject, MatrixGridChildMonitor> childToMonitorMap;
        private readonly MatrixGridChildConverter converter;

        #endregion // Fields
    }
}
