namespace TMP.WORK.AramisChetchiki.Charts
{
    using System;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;

    /// <summary>
    /// Легенда диаграммы
    /// </summary>
    public partial class Legend : UserControl
    {
        public Legend()
        {
            this.InitializeComponent();

            DependencyPropertyDescriptor dpd = DependencyPropertyDescriptor.FromProperty(PieChartLayout.ObjectPropertyProperty, typeof(PieChart));
            dpd.AddValueChanged(this, this.ObjectPropertyChanged);

            this.DataContextChanged += new DependencyPropertyChangedEventHandler(this.DataContextChangedHandler);
        }

        #region dependency properties

        /// <summary>
        /// Свойство объекта по которому строится диаграмма
        /// </summary>
        public string ObjectProperty
        {
            get => PieChartLayout.GetObjectProperty(this);
            set => PieChartLayout.SetObjectProperty(this, value);
        }

        /// <summary>
        /// Класс, выбирающий цвет рядов диграммы
        /// </summary>
        public IColorSelector ColorSelector
        {
            get => PieChartLayout.GetColorSelector(this);
            set => PieChartLayout.SetColorSelector(this, value);
        }

        #endregion

        #region property change handlers

        /// <summary>
        /// Отслеживание изменений контекста данных
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataContextChangedHandler(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != null)
            {
                if (this.DataContext is INotifyCollectionChanged changed)
                {
                    changed.CollectionChanged += new NotifyCollectionChangedEventHandler(this.BoundCollectionChanged);
                }

                this.ObserveBoundCollectionChanges();
            }
        }

        #endregion

        #region event handlers

        /// <summary>
        /// Отслеживание изменения коллекции
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BoundCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.RefreshView();
            this.ObserveBoundCollectionChanges();
        }

        /// <summary>
        /// Обработчик изменения свойства ObjectProperty
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ObjectPropertyChanged(object sender, EventArgs e)
        {
            this.RefreshView();
        }

        /// <summary>
        /// Перебор всей коллекции и подписка на изменение свойств
        /// </summary>
        private void ObserveBoundCollectionChanges()
        {
            CollectionView myCollectionView = (CollectionView)CollectionViewSource.GetDefaultView(this.DataContext);

            foreach (object item in myCollectionView)
            {
                if (item is INotifyPropertyChanged observable)
                {
                    observable.PropertyChanged += new PropertyChangedEventHandler(this.ItemPropertyChanged);
                }
            }
        }

        /// <summary>
        /// Обработчик измения свойств элеметов коллекции
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // если это свойство, по которому построена диаграмма, обновляем её
            if (string.Equals(e.PropertyName, this.ObjectProperty, AppSettings.StringComparisonMethod))
            {
                this.RefreshView();
            }
        }

        #endregion

        /// <summary>
        /// Обновление диаграммы
        /// </summary>
        private void RefreshView()
        {
            // указываем, что данные изменились и необходимо обновить привязку данных
            object context = this.legend.DataContext;
            if (context != null)
            {
                this.legend.DataContext = null;
                this.legend.DataContext = context;
            }
        }
    }
}
