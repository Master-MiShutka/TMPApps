namespace DataGridWpf
{
    using System.ComponentModel;
    using System.Windows;
    using System.Windows.Controls;

    public class CollectionViewNavigator : ContentControl
    {
        public CollectionViewNavigator()
        {
        }

        [Bindable(true)]
        public ICollectionView PagingCollectionView
        {
            get => (ICollectionView)this.GetValue(PagingCollectionViewProperty);
            set
            {
                if (value == null)
                {
                    this.ClearValue(PagingCollectionViewProperty);
                }
                else
                {
                    this.SetValue(PagingCollectionViewProperty, value);
                }
            }
        }

        public static readonly DependencyProperty PagingCollectionViewProperty =
            DependencyProperty.Register(
                nameof(PagingCollectionView),
                typeof(ICollectionView),
                typeof(CollectionViewNavigator),
                new FrameworkPropertyMetadata(default));
    }
}
