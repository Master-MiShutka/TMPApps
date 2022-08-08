namespace TMP.PrintEngine.Controls.ProgressDialog
{
    using System.Windows.Controls;
    using TMP.PrintEngine.ViewModels;

    /// <summary>
    /// Interaction logic for ProgressDialogView.xaml
    /// </summary>
    public partial class ProgressDialogView : UserControl, IProgressDialogView
    {
        public ProgressDialogView()
        {
            this.InitializeComponent();
        }

        private IProgressDialogViewModel presenter;

        public IViewModel ViewModel
        {
            set
            {
                this.presenter = value as IProgressDialogViewModel;
                this.DataContext = this.presenter;
            }
        }
    }
}