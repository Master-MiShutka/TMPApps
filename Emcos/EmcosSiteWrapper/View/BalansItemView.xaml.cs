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
    using TMP.Common.Logger;
    /// <summary>
    /// Interaction logic for BalansItemView.xaml
    /// </summary>
    public partial class BalansItemView : Window, IStateObject
    {
        #region | Реализация IStateObject |
        public string Log { get { return null; } set { throw new NotImplementedException(); } }
        private int _progress = 0;
        public int Progress
        {
            get { return _progress; }
            set
            {
                _progress = value;
                RaisePropertyChanged(nameof(Progress));
            }
        }
        private State _state = State.Busy;
        public State State
        {
            get { return _state; }
            set
            {
                _state = value;
                RaisePropertyChanged(nameof(State));
            }
        }

        #endregion

        #region | Реализация INotifyPropertyChanged |

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged(string propertyName)
        {
            var e = PropertyChanged;
            if (e != null)
                e(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }

        #endregion

        private Model.DatePeriod _period;

        public BalansItemView(Model.DatePeriod period)
        {
            _period = period;
            InitializeComponent();
            State = State.Idle;
        }

        private void TextBox_Error(object sender, ValidationErrorEventArgs e)
        {
            if (e.Action == ValidationErrorEventAction.Added)
            {
                e.Error.ErrorContent = "Введённое значение не является числом!";
                e.Handled = true;
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape) Close();
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            if (rootGrid.DataContext == null) return;
            var selectedItem = rootGrid.DataContext as Model.Balans.IBalansItem;
            if (selectedItem != null)
            {
                if (selectedItem is Model.Balans.Fider)
                    EMinusCorrection.Focus();
                else
                    EPlusCorrection.Focus();
            }
        }

        private void UpdateData_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn == null) return;
            //string dir = btn.Tag as String;
            //if (String.IsNullOrWhiteSpace(dir)) return;

            var item = rootGrid.DataContext as Model.Balans.IBalansItem;

            wait.Message = "Пожалуйста, подождите ...\nОбновление данных.";
            var cts = new System.Threading.CancellationTokenSource();

            var task = EmcosSiteWrapper.Instance.ExecuteAction(cts, () =>
            {
                State = State.Busy;

                try
                {
                    U.GetDaylyArchiveDataForItem(_period.StartDate, _period.EndDate, item, cts);
                }
                catch (Exception ex)
                {
                    App.ToLogError(String.Format("Обновление данных по точке ({0}). Произошла ошибка: {1}", item.Name, ex.Message));
                    State = State.Idle;
                }

                App.UIAction(() =>
                  {
                      Progress = 0;
                  });

                State = State.Idle;
            });
        }
    }
}
