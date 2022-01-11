using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace TMP.Work.Emcos
{
    public static class U
    {
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
        /// <summary>
        /// Окно сообщения с полем для ввода
        /// </summary>
        /// <param name="title">Заголовок окна</param>
        /// <param name="message">Сообщение</param>
        /// <param name="initText">Первоначальный текст в поле</param>
        /// <returns>Введенный пользователем текст</returns>
        public static string InputBox(string title, string message, string initText = null)
        {
            var w = new Window
            {
                Owner = EmcosSiteWrapperApp.Current.MainWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                WindowStyle = WindowStyle.SingleBorderWindow,
                Title = title,
                TaskbarItemInfo = new System.Windows.Shell.TaskbarItemInfo(),
                SizeToContent = SizeToContent.WidthAndHeight,
                ResizeMode = ResizeMode.NoResize,
                ShowInTaskbar = false,
                FontFamily = new System.Windows.Media.FontFamily("Calibri,Verdana,Tahoma"),
                Icon = EmcosSiteWrapperApp.Current.MainWindow.Icon
            };
            w.SetResourceReference(Window.FontSizeProperty, "SubHeaderFontSize");
            w.SetValue(System.Windows.Media.TextOptions.TextFormattingModeProperty, System.Windows.Media.TextFormattingMode.Ideal);
            w.TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Indeterminate;
            var grid = new Grid { Margin = new Thickness(10d) };
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
            w.Content = grid;

            var tbMessage = new TextBlock
            {
                TextAlignment = TextAlignment.Center,
                Text = message
            };
            tbMessage.SetResourceReference(Window.FontSizeProperty, "SubHeaderFontSize");
            Grid.SetRow(tbMessage, 0);

            var tb = new TextBox
            {
                TextAlignment = TextAlignment.Center,
                Text = initText
            };
            tb.SetResourceReference(Window.FontSizeProperty, "SubHeaderFontSize");
            Grid.SetRow(tb, 1);

            var btnSave = new Button
            {
                Padding = new Thickness(10, 2, 10, 2),
                Margin = new Thickness(0, 10, 10, 2),
                IsDefault = true
            };
            btnSave.SetResourceReference(Window.FontSizeProperty, "SubHeaderFontSize");
            btnSave.Content = "Сохранить";
            btnSave.Click += (s, e) =>
            {
                w.DialogResult = true;
                w.Close();
            };

            var btnCancel = new Button
            {
                Padding = new Thickness(10, 2, 10, 2),
                Margin = new Thickness(10, 10, 0, 2),
                IsCancel = true
            };
            btnCancel.SetResourceReference(Window.FontSizeProperty, "SubHeaderFontSize");
            btnCancel.Content = "Отменить";

            var sp = new StackPanel
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                Orientation = Orientation.Horizontal
            };
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
