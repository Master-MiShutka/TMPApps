using System;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.ComponentModel;

namespace TMP.Work.Emcos
{
    public static class U
    {
        public static string GetExceptionDetails(Exception exp)
        {
            var messageBoxText = string.Empty;

            try
            {
                // Write Message tree of inner exception into textual representation
                messageBoxText = exp.Message;

                var innerEx = exp.InnerException;

                for (int i = 0; innerEx != null; i++, innerEx = innerEx.InnerException)
                {
                    var spaces = string.Empty;

                    for (int j = 0; j < i; j++)
                        spaces += "  ";

                    messageBoxText += "\n" + spaces + "└─>" + innerEx.Message;
                }
            }
            catch
            {
            }
            return messageBoxText;
        }

        public static AsyncCallback SyncContextCallback(AsyncCallback callback)
        {
            // Фиксируем производный от SyncronizationContext.Current объект вызывающего потока
            var sc = SynchronizationContext.Current;

            // При отсутствии контекста синхронизации возвращаем переданное в метод
            if (sc == null) return callback;

            // Возвращаем делегат, который отправляет в фиксированный контекст синхронизации
            // метод, передающий в  исходный вызов AsyncCallback аргумент IAsyncResult
            return asyncResult => sc.Post(result => callback((IAsyncResult)result), asyncResult);
        }

        /*****************************************************************/
        public static string GetData(FrameworkElement sender, EmcosSiteWrapperMethod method, string parametr, Func<string, bool> postAction)
        {
            var stateObj = sender as IStateObject;
            if (stateObj == null)
                throw new ArgumentException("Not a IStateObject");
            var data = String.Empty;
            stateObj.State = State.Busy;
            try
            {
                var response = string.Empty;
                var task = method(parametr);
                if (task == null) return null;

                task.
                ContinueWith<String>((t) =>
                {
                    MessageBox.Show(t.Exception.InnerException.Message);
                    stateObj.State = State.Idle;
                    return null;
                },
                    System.Threading.CancellationToken.None,
                    System.Threading.Tasks.TaskContinuationOptions.OnlyOnFaulted,
                    System.Threading.Tasks.TaskScheduler.Current
                    );
                task.
                ContinueWith<String>(t =>
                {
                    response = t.Result;

                    if (String.IsNullOrWhiteSpace(response))
                    {
                        sender.Dispatcher.Invoke((Action)(() =>
                                MessageBox.Show("Не удалось получить данные!", "Проблема с доступом", MessageBoxButton.OK, MessageBoxImage.Exclamation)));
                    }
                    if (response.Contains("result=1"))
                    {
                        if (EmcosSiteWrapper.Instance.Login(EmcosSiteWrapper.AuthorizationType.Login) == true)
                        {
                            if (EmcosSiteWrapper.Instance.Login(EmcosSiteWrapper.AuthorizationType.GetRights) == false)
                                sender.Dispatcher.Invoke((Action)(() =>
                                MessageBox.Show("Не удалось авторизоваться!", "Нет доступа", MessageBoxButton.OK, MessageBoxImage.Exclamation)));
                        }
                        else
                            response = GetData(sender, method, parametr, postAction);
                    }
                    else
                    {
                        if (response.Contains("result=0") == false)
                            sender.Dispatcher.Invoke((Action)(() =>
                                MessageBox.Show("Данные не получены!\nКод результата:\n" + response, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Exclamation)));
                        else
                        {
                            postAction(response);
                        }
                    }
                    stateObj.State = State.Idle;
                    return response;
                },
                    System.Threading.CancellationToken.None,
                    System.Threading.Tasks.TaskContinuationOptions.OnlyOnRanToCompletion,
                    System.Threading.Tasks.TaskScheduler.Current
                    );

                if (String.IsNullOrWhiteSpace(response))
                    response = task.Result;

                return response;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                stateObj.State = State.Idle;
                return null;
            }
        }

        public static string InputBox(string title, string message, string initText = null)
        {
            var w = new Window();
            w.Owner = App.Current.MainWindow;
            w.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            w.WindowStyle = WindowStyle.SingleBorderWindow;
            w.Title = title;
            w.TaskbarItemInfo = new System.Windows.Shell.TaskbarItemInfo();
            w.TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Indeterminate;
            w.SizeToContent = SizeToContent.WidthAndHeight;
            w.ResizeMode = ResizeMode.NoResize;
            w.ShowInTaskbar = false;
            w.FontFamily = new System.Windows.Media.FontFamily("Calibri,Verdana,Tahoma");
            w.SetResourceReference(Window.FontSizeProperty, "SubHeaderFontSize");
            w.SetValue(System.Windows.Media.TextOptions.TextFormattingModeProperty, System.Windows.Media.TextFormattingMode.Ideal);
            w.Icon = App.Current.MainWindow.Icon;
            var grid = new Grid();
            grid.Margin = new Thickness(10d);
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
            w.Content = grid;

            var tbMessage = new TextBlock();
            tbMessage.TextAlignment = TextAlignment.Center;
            tbMessage.Text = message;
            tbMessage.SetResourceReference(Window.FontSizeProperty, "SubHeaderFontSize");
            Grid.SetRow(tbMessage, 0);

            var tb = new TextBox();
            tb.TextAlignment = TextAlignment.Center;
            tb.Text = initText;
            tb.SetResourceReference(Window.FontSizeProperty, "SubHeaderFontSize");
            Grid.SetRow(tb, 1);

            var btnSave = new Button();
            btnSave.Padding = new Thickness(10, 2, 10, 2);
            btnSave.Margin = new Thickness(0, 10, 10, 2);
            btnSave.IsDefault = true;
            btnSave.SetResourceReference(Window.FontSizeProperty, "SubHeaderFontSize");
            btnSave.Content = "Сохранить";
            btnSave.Click += (s, e) =>
            {
                w.DialogResult = true;
                w.Close();
            };

            var btnCancel = new Button();
            btnCancel.Padding = new Thickness(10, 2, 10, 2);
            btnCancel.Margin = new Thickness(10, 10, 0, 2);
            btnCancel.IsCancel = true;
            btnCancel.SetResourceReference(Window.FontSizeProperty, "SubHeaderFontSize");
            btnCancel.Content = "Отменить";

            var sp = new StackPanel();
            sp.HorizontalAlignment = HorizontalAlignment.Center;
            sp.Orientation = Orientation.Horizontal;
            sp.Children.Add(btnSave);
            sp.Children.Add(btnCancel);
            Grid.SetRow(sp, 2);

            grid.Children.Add(tbMessage);
            grid.Children.Add(tb);
            grid.Children.Add(sp);

            w.SetValue(System.Windows.Input.FocusManager.FocusedElementProperty, tb);

            var result = w.ShowDialog();

            if (result.HasValue)
                return tb.Text;
            else
                return null;
        }
    }
}
