using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.Windows.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Threading.Tasks;


namespace TMP.Wpf.CommonControls.Dialogs
{
    class ModalDialogHelper : INotifyPropertyChanged, IModalDialogHelper
    {
        private readonly Queue<TaskCompletionSource<MessageBoxResult>> _waits =
            new Queue<TaskCompletionSource<MessageBoxResult>>();
        private readonly object syncObject = new object();
        private readonly Dispatcher _dispatcher = Dispatcher.CurrentDispatcher;

        private string _text;

        private bool _isVisible;

        public async Task ShowAsync(string text)
        {
            List<TaskCompletionSource<MessageBoxResult>> previousWaits;
            TaskCompletionSource<MessageBoxResult> currentWait;

            lock (syncObject)
            {
                //Запоминаем список задач, которые надо ожидать
                previousWaits = _waits.ToList();

                //Создаем задачу для данного конкретного сообщения
                currentWait = new TaskCompletionSource<MessageBoxResult>();
                _waits.Enqueue(currentWait);
            }

            //Ждем завершения задач, которые уже есть в очереди
            foreach (var wait in previousWaits)
            {
                await wait.Task;
            }

            //Этот блок должен быть выполнен на основном потоке
            _dispatcher.Invoke(() =>
            {
                Text = text;
                IsVisible = true;
            });

            await currentWait.Task;
        }

        #region | IModalDialogHelper Implementation |

        public string Text
        {
            get => _text;
            set
            {
                _text = value;
                OnPropertyChanged(nameof(Text));
            }
        }

        public bool IsVisible
        {
            get => _isVisible;
            set
            {
                _isVisible = value;
                OnPropertyChanged(nameof(IsVisible));
            }
        }

        public ICommand CloseCommand { get; set; }

        public void Show(string text)
        {
            Text = text;
            IsVisible = true;
        }
        public void Close()
        {
            IsVisible = false;

            TaskCompletionSource<MessageBoxResult> wait;

            lock (syncObject)
            {
                //Убрать текущую задачу из очереди
                wait = _waits.Dequeue();
            }

            //В полном решении этот результат будет устанавливаться в зависимости от нажатой кнопки
            wait.SetResult(MessageBoxResult.OK);
        }

        #endregion

        #region | INotifyPropertyChanged Implementation |

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises the PropertyChanged event if needed.
        /// </summary>
        /// <param name="propertyName">The name of the property that changed.</param>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = System.Threading.Interlocked.CompareExchange(ref PropertyChanged, null, null);
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion | INotifyPropertyChanged |
    }
}
