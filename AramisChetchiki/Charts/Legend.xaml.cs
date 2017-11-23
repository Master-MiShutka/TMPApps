using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Collections.Specialized;

namespace TMP.WORK.AramisChetchiki.Charts
{
    /// <summary>
    /// Легенда диаграммы
    /// </summary>
    public partial class Legend : UserControl
    {
        public Legend()
        {
            InitializeComponent();

            DependencyPropertyDescriptor dpd = DependencyPropertyDescriptor.FromProperty(PieChartLayout.ObjectPropertyProperty, typeof(PieChart));
            dpd.AddValueChanged(this, ObjectPropertyChanged);

            this.DataContextChanged += new DependencyPropertyChangedEventHandler(DataContextChangedHandler);
        }

        #region dependency properties

        /// <summary>
        /// Свойство объекта по которому строится диаграмма
        /// </summary>
        public String ObjectProperty
        {
            get { return PieChartLayout.GetObjectProperty(this); }
            set { PieChartLayout.SetObjectProperty(this, value); }
        }

        /// <summary>
        /// Класс, выбирающий цвет рядов диграммы
        /// </summary>
        public IColorSelector ColorSelector
        {
            get { return PieChartLayout.GetColorSelector(this); }
            set { PieChartLayout.SetColorSelector(this, value); }
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
            if (this.DataContext is INotifyCollectionChanged)
            {
                INotifyCollectionChanged observable = (INotifyCollectionChanged)this.DataContext;
                observable.CollectionChanged += new NotifyCollectionChangedEventHandler(BoundCollectionChanged);
            }

            ObserveBoundCollectionChanges();
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
            RefreshView();
            ObserveBoundCollectionChanges();
        }

        /// <summary>
        /// Обработчик изменения свойства ObjectProperty
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ObjectPropertyChanged(object sender, EventArgs e)
        {
            RefreshView();
        }

        /// <summary>
        /// Перебор всей коллекции и подписка на изменение свойств
        /// </summary>
        private void ObserveBoundCollectionChanges()
        {
            CollectionView myCollectionView = (CollectionView)CollectionViewSource.GetDefaultView(this.DataContext);

            foreach (object item in myCollectionView)
            {
                if (item is INotifyPropertyChanged)
                {
                    INotifyPropertyChanged observable = (INotifyPropertyChanged)item;
                    if (observable != null)
                        observable.PropertyChanged += new PropertyChangedEventHandler(ItemPropertyChanged);
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
            if (e.PropertyName.Equals(ObjectProperty))
            {
                RefreshView();
            }
        }

        #endregion

        /// <summary>
        /// Обновление диаграммы
        /// </summary>
        private void RefreshView()
        {
            // указываем, что данные изменились и необходимо обновить привязку данных
            object context = legend.DataContext;
            if (context != null)
            {
                legend.DataContext = null;
                legend.DataContext = context;
            }
        }
    }
}
