namespace TMP.PrintEngine.ViewModels
{
    using System.Windows;
    using TMP.PrintEngine.Views;

    public abstract class AViewModel : DependencyObject, IViewModel
    {
        protected AViewModel(IView view)
        {
            this.View = view;
            view.ViewModel = this;
        }
        #region IViewModel Members

        public IView View
        {
            get => (IView)this.GetValue(ViewProperty);

            set => this.SetValue(ViewProperty, value);
        }

        public static readonly DependencyProperty ViewProperty =
           DependencyProperty.Register("View", typeof(IView), typeof(AViewModel));
        #endregion
    }
}
