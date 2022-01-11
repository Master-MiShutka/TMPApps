namespace TMP.WORK.AramisChetchiki.Controls
{
    using System.Windows.Input;
    using TMP.Shared.Commands;

    /// <summary>
    /// Interaction logic for StringFilter.xaml
    /// </summary>
    public partial class StringFilter : BaseFilterControl
    {
        public StringFilter()
        {
            this.CommandChangeMode = new DelegateCommand<object>((o) =>
            {
                this.filterView.ViewModel.Mode = (ItemsFilter.Model.StringFilterMode)o;
            });

            this.InitializeComponent();
        }

        public ICommand CommandChangeMode { get; }
    }
}
