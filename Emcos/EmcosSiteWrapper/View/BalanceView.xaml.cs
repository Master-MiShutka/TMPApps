namespace TMP.Work.Emcos.View
{
    /// <summary>
    /// Interaction logic for BalanceView.xaml
    /// </summary>
    public partial class BalanceView : TMPApplication.CustomWpfWindow.WindowWithDialogs
    {
        public BalanceView()
        {
            EmcosSiteWrapperApp.LogInfo("Инициализация BalanceView");
            InitializeComponent();
        }
    }
}