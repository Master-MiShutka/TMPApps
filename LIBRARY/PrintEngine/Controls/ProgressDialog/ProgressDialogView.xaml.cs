using System.Windows.Controls;
using TMP.PrintEngine.ViewModels;

namespace TMP.PrintEngine.Controls.ProgressDialog
{
    /// <summary>
    /// Interaction logic for ProgressDialogView.xaml
    /// </summary>
    public partial class ProgressDialogView : UserControl, IProgressDialogView
    {
        public ProgressDialogView()
        {
            InitializeComponent();
        }

        private IProgressDialogViewModel _presenter;
        public IViewModel ViewModel
        {
            set
            {
                _presenter = value as IProgressDialogViewModel;
                DataContext = _presenter;
            }
        }
    }
}