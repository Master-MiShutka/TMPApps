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
using System.Windows.Shapes;

namespace TMP.Work.Emcos.View
{
    using TMPApplication;
    using TMPApplication.CustomWpfWindow;
    using TMPApplication.WpfDialogs.Contracts;

    /// <summary>
    /// Interaction logic for BalanceItemView.xaml
    /// </summary>
    public partial class BalanceItemView : WindowWithDialogs
    {
        private Model.DatePeriod _period;

        public BalanceItemView(Model.DatePeriod period)
        {
            _period = period;
            InitializeComponent();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                Close();
        }

        private void UpdateData_Click(object sender, RoutedEventArgs e)
        {

            var btn = sender as Button;
            if (btn == null) return;
            //string dir = btn.Tag as String;
            //if (String.IsNullOrWhiteSpace(dir)) return;

            var item = rootGrid.DataContext as Model.Balance.IBalanceItem;

            //wait.Message = "Пожалуйста, подождите ...\nОбновление данных.";
            var cts = new System.Threading.CancellationTokenSource();

            var task = EmcosSiteWrapper.Instance.ExecuteAction(cts, () =>
            {
                //State = State.Busy;

                try
                {
                    Emcos.Utils.GetBalanceItemArchiveData(item, _period.StartDate, _period.EndDate, cts);
                }
                catch (Exception ex)
                {
                    EmcosSiteWrapperApp.LogError(String.Format("Обновление данных по точке ({0}). Произошла ошибка: {1}", item.Name, ex.Message));
                    //State = State.Idle;
                }

                DispatcherExtensions.InUi(() =>
                {
                    //Progress = 0;
                });

                //State = State.Idle;
            });
        }
    }
}
