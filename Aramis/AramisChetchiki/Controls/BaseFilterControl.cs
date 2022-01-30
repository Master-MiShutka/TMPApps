namespace TMP.WORK.AramisChetchiki.Controls
{
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;

    public class BaseFilterControl : UserControl, System.ComponentModel.INotifyPropertyChanged
    {
        private bool isFilterActive;

        public BaseFilterControl()
        {
        }

        #region Properties

        public bool IsFilterActive { get => this.isFilterActive; private set => this.SetProperty(ref this.isFilterActive, value); }

        #endregion

        #region Dependency properties

        #region Filter

        public ItemsFilter.Model.IFilter Filter
        {
            get => (ItemsFilter.Model.IFilter)this.GetValue(FilterProperty);
            set => this.SetValue(FilterProperty, value);
        }

        public static readonly DependencyProperty FilterProperty =
            DependencyProperty.Register(
                nameof(Filter),
                typeof(ItemsFilter.Model.IFilter),
                typeof(BaseFilterControl),
                new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnFilterChanged)));

        private static void OnFilterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            BaseFilterControl target = (BaseFilterControl)d;

            if (target.Filter != null)
            {
                target.Filter.PropertyChanged -= target.Filter_PropertyChanged;
            }

            ItemsFilter.Model.IFilter filter = (ItemsFilter.Model.IFilter)e.NewValue;

            if (filter is ItemsFilter.Model.IPropertyFilter ipf)
            {
                System.ComponentModel.ItemPropertyInfo p = ipf.PropertyInfo;
                target.FilterPropertyName = ipf.PropertyInfo.Name;
            }

            if (filter is ItemsFilter.Model.IStringFilter sf)
            {
                target.FilterPropertyName = sf.PropertyInfo.Name;
            }

            if (target.Filter != null)
            {
                target.Filter.PropertyChanged += target.Filter_PropertyChanged;
            }
        }

        private void Filter_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ItemsFilter.Model.IFilter.IsActive))
            {
                this.IsFilterActive = (bool)this.Filter?.IsActive;
                this.RaisePropertyChanged(nameof(this.IsFilterActive));
            }
        }

        #endregion

        #region FilterPresenter

        public ItemsFilter.FilterPresenter FilterPresenter
        {
            get => (ItemsFilter.FilterPresenter)this.GetValue(FilterPresenterProperty);
            set => this.SetValue(FilterPresenterProperty, value);
        }

        public static readonly DependencyProperty FilterPresenterProperty =
            DependencyProperty.Register(
                nameof(FilterPresenter),
                typeof(ItemsFilter.FilterPresenter),
                typeof(BaseFilterControl),
                new FrameworkPropertyMetadata(
                    null,
                    new PropertyChangedCallback(OnFilterPresenterChanged)));

        private static void OnFilterPresenterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            BaseFilterControl target = (BaseFilterControl)d;
            ItemsFilter.FilterPresenter filterPresenter = (ItemsFilter.FilterPresenter)e.NewValue;

            target.SetCurrentValue(
                FilterProperty,
                (ItemsFilter.Model.IMultiValueFilter)filterPresenter.TryGetFilter(
                    target.FilterPropertyName,
                    new ItemsFilter.Initializer.EqualFilterInitializer()));
        }

        #endregion

        #region FilterPropertyName

        public string FilterPropertyName
        {
            get => (string)this.GetValue(FilterPropertyNameProperty);
            set => this.SetValue(FilterPropertyNameProperty, value);
        }

        public static readonly DependencyProperty FilterPropertyNameProperty =
            DependencyProperty.Register(
                nameof(FilterPropertyName),
                typeof(string),
                typeof(BaseFilterControl),
                new FrameworkPropertyMetadata("<?>", new PropertyChangedCallback(OnFilterPropertyNameChanged)));

        private static void OnFilterPropertyNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            BaseFilterControl target = (BaseFilterControl)d;
            string filterPropertyName = (string)e.NewValue;

            if (target.FilterPresenter == null)
            {
                return;
            }

            if (target.FilterPropertyName == filterPropertyName)
            {
                return;
            }

            target.SetCurrentValue(
                FilterProperty,
                target.FilterPresenter.TryGetFilter(
                    filterPropertyName,
                    new ItemsFilter.Initializer.EqualFilterInitializer()) as ItemsFilter.Model.IMultiValueFilter);
        }

        #endregion

        #endregion

        #region Члены INotifyPropertyChanged

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        public bool SetProperty<T>(ref T storage, T value, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            if (!System.Collections.Generic.EqualityComparer<T>.Default.Equals(storage, value))
            {
                storage = value;
                this.RaisePropertyChanged(propertyName);
                return true;
            }

            return false;
        }

        protected virtual void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            this.VerifyPropertyName(propertyName);

            this.PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }

        [System.Diagnostics.Conditional("DEBUG")]
        [System.Diagnostics.DebuggerStepThrough]
        protected void VerifyPropertyName(string propertyName)
        {
            System.Type myType = this.GetType();
            if (!string.IsNullOrEmpty(propertyName)
                && myType.GetProperty(propertyName) == null)
            {
                throw new System.ArgumentException("Property not found", propertyName);
            }
        }

        #endregion Члены INotifyPropertyChanged
    }
}
