using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace TMP.Wpf.CommonControls.Dialogs
{
    public abstract class BaseTMPDialog : ContentControl
    {
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(BaseTMPDialog), new PropertyMetadata(default(string)));
        public static readonly DependencyProperty DialogTopProperty = DependencyProperty.Register("DialogTop", typeof(object), typeof(BaseTMPDialog), new PropertyMetadata(null));
        public static readonly DependencyProperty DialogBottomProperty = DependencyProperty.Register("DialogBottom", typeof(object), typeof(BaseTMPDialog), new PropertyMetadata(null));

        protected TMPDialogSettings DialogSettings { get; private set; }

        /// <summary>
        /// Возвращает/задает заголовок диалога
        /// </summary>
        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        /// <summary>
        /// Возвращает/задает произвольное содержимое сверху диалога
        /// </summary>
        public object DialogTop
        {
            get => GetValue(DialogTopProperty);
            set => SetValue(DialogTopProperty, value);
        }

        /// <summary>
        /// Возвращает/задает произвольное содержимое снизу диалога
        /// </summary>
        public object DialogBottom
        {
            get => GetValue(DialogBottomProperty);
            set => SetValue(DialogBottomProperty, value);
        }

        internal SizeChangedEventHandler SizeChangedHandler { get; set; }

        static BaseTMPDialog()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(BaseTMPDialog), new FrameworkPropertyMetadata(typeof(BaseTMPDialog)));
        }

        /// <summary>
        /// Инициализирует новый экземпляр TMP.Wpf.CommonControls.Dialogs.BaseTMPDialog
        /// </summary>
        /// <param name="owningWindow">Родительское окно</param>
        /// <param name="settings">Настройки сообщений диалога</param>
        protected BaseTMPDialog(TMPWindow owningWindow, TMPDialogSettings settings)
        {
            DialogSettings = settings ?? owningWindow.TMPDialogOptions;

            OwningWindow = owningWindow;

            Initialize();
        }

        /// <summary>
        /// Инициализирует новый экземпляр TMP.Wpf.CommonControls.Dialogs.BaseTMPDialog
        /// </summary>
        protected BaseTMPDialog()
        {
            DialogSettings = new TMPDialogSettings();

            Initialize();
        }

        private void Initialize()
        {
            ThemeManager.IsThemeChanged += ThemeManager_IsThemeChanged;
            this.Unloaded += BaseTMPDialog_Unloaded;

            HandleTheme();

            this.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri("pack://application:,,,/TMP.Wpf.CommonControls;component/Themes/Dialogs/BaseTMPDialog.xaml") });
        }

        private void BaseTMPDialog_Unloaded(object sender, RoutedEventArgs e)
        {
            this.Unloaded -= BaseTMPDialog_Unloaded;
        }

        private void ThemeManager_IsThemeChanged(object sender, OnThemeChangedEventArgs e)
        {
            HandleTheme();
        }

        private void HandleTheme()
        {
            if (DialogSettings != null)
            {
                var windowTheme = DetectTheme(this);
                var theme = windowTheme.Item1;
                var windowAccent = windowTheme.Item2;

                switch (DialogSettings.ColorScheme)
                {
                    case TMPDialogColorScheme.Theme:
                        ThemeManager.ChangeAppStyle(this.Resources, windowAccent, theme);
                        this.SetValue(BackgroundProperty, ThemeManager.GetResourceFromAppStyle(OwningWindow ?? Application.Current.MainWindow, "WhiteColorBrush"));
                        this.SetValue(ForegroundProperty, ThemeManager.GetResourceFromAppStyle(OwningWindow ?? Application.Current.MainWindow, "BlackBrush"));
                        break;

                    case TMPDialogColorScheme.Inverted:
                        var inverseTheme = ThemeManager.GetInverseAppTheme(theme);
                        if (inverseTheme == null)
                        {
                            throw new InvalidOperationException("The inverse dialog theme only works if the window theme abides the naming convention. " +
                                                                "See ThemeManager.GetInverseAppTheme for more infos");
                        }

                        ThemeManager.ChangeAppStyle(this.Resources, windowAccent, inverseTheme);
                        this.SetValue(BackgroundProperty, ThemeManager.GetResourceFromAppStyle(OwningWindow ?? Application.Current.MainWindow, "BlackColorBrush"));
                        this.SetValue(ForegroundProperty, ThemeManager.GetResourceFromAppStyle(OwningWindow ?? Application.Current.MainWindow, "WhiteColorBrush"));
                        break;

                    case TMPDialogColorScheme.Accented:
                        ThemeManager.ChangeAppStyle(this.Resources, windowAccent, theme);
                        this.SetValue(BackgroundProperty, ThemeManager.GetResourceFromAppStyle(OwningWindow ?? Application.Current.MainWindow, "HighlightBrush"));
                        this.SetValue(ForegroundProperty, ThemeManager.GetResourceFromAppStyle(OwningWindow ?? Application.Current.MainWindow, "IdealForegroundColorBrush"));
                        break;
                }
            }
        }

        private static Tuple<AppTheme, Accent> DetectTheme(BaseTMPDialog dialog)
        {
            if (dialog == null)
                return null;

            // first look for owner
            var window = dialog.TryFindParent<TMPWindow>();
            var theme = window != null ? ThemeManager.DetectAppStyle(window) : null;
            if (theme != null && theme.Item2 != null)
                return theme;

            // second try, look for main window
            if (Application.Current != null)
            {
                var mainWindow = Application.Current.MainWindow as TMPWindow;
                theme = mainWindow != null ? ThemeManager.DetectAppStyle(mainWindow) : null;
                if (theme != null && theme.Item2 != null)
                    return theme;

                // oh no, now look at application resource
                theme = ThemeManager.DetectAppStyle(Application.Current);
                if (theme != null && theme.Item2 != null)
                    return theme;
            }
            return null;
        }

        /// <summary>
        /// Ожидает пока диалог не будет готов к взаимодействию
        /// </summary>
        /// <returns>Задача представляющая операцию и ее статус</returns>
        public Task WaitForLoadAsync()
        {
            Dispatcher.VerifyAccess();

            if (this.IsLoaded) return new Task(() => { });

            if (!DialogSettings.AnimateShow)
                this.Opacity = 1.0; //skip the animation

            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();

            RoutedEventHandler handler = null;
            handler = (sender, args) =>
            {
                this.Loaded -= handler;

                this.Focus();

                tcs.TrySetResult(null);
            };

            this.Loaded += handler;

            return tcs.Task;
        }

        /// <summary>
        /// Запрос на внешнее закрытие открытого диалога. Вызовет исключение, если диалог внутри TMPWindow
        /// </summary>
        public Task RequestCloseAsync()
        {
            if (OnRequestClose())
            {
                // Технически, диалог /всегда/ внутри TMPWindow
                // Если диалог внутри TMPWindow, не созданного внешним API
                if (ParentDialogWindow == null)
                {
                    // Создан внутри TMPWindow, созданного пользователем
                    return DialogManager.HideTMPDialogAsync(OwningWindow, this);
                }

                // Создан внутри окна TMPWindow, созданного внешним API
                return _WaitForCloseAsync().ContinueWith(x =>
                {
                    ParentDialogWindow.Dispatcher.Invoke(new Action(() =>
                    {
                        ParentDialogWindow.Close();
                    }));
                });
            }
            return Task.Factory.StartNew(() => { });
        }

        protected internal virtual void OnShown()
        {
        }

        protected internal virtual void OnClose()
        {
            if (ParentDialogWindow != null) //this is only set when a dialog is shown (externally) in it's OWN window.
                ParentDialogWindow.Close();
        }

        /// <summary>
        /// Последний шанс виртуального метода остановить закрытие диалога
        /// </summary>
        /// <returns></returns>
        protected internal virtual bool OnRequestClose()
        {
            return true; // разрешает закрытие
        }

        /// <summary>
        /// Возвращает окно, являющееся владельцем текущего диалога ЕСЛИ И ТОЛЬКО ЕСЛИ диалог отображается внешне
        /// </summary>
        protected internal Window ParentDialogWindow { get; internal set; }

        /// <summary>
        /// Возвращает окно, являющееся владельцем текущего диалога ЕСЛИ И ТОЛЬКО ЕСЛИ диалог отображается внутри окна
        /// </summary>
        protected internal TMPWindow OwningWindow { get; internal set; }

        public Task _WaitForCloseAsync()
        {
            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();

            if (DialogSettings.AnimateHide)
            {
                Storyboard closingStoryboard = this.Resources["DialogCloseStoryboard"] as Storyboard;

                if (closingStoryboard == null)
                    throw new InvalidOperationException("Unable to find the dialog closing storyboard. Did you forget to add BaseTMPDialog.xaml to your merged dictionaries?");

                EventHandler handler = null;
                handler = (sender, args) =>
                {
                    closingStoryboard.Completed -= handler;

                    tcs.TrySetResult(null);
                };

                closingStoryboard = closingStoryboard.Clone();

                closingStoryboard.Completed += handler;

                closingStoryboard.Begin(this);
            }
            else
            {
                this.Opacity = 0.0;
                tcs.TrySetResult(null); //skip the animation
            }

            return tcs.Task;
        }

        internal void Close()
        {
            OnClose();
            _WaitForCloseAsync().ContinueWith(y =>
            {
                if (CloseHandler != null)
                    CloseHandler(this, EventArgs.Empty);
            });
        }

        internal EventHandler CloseHandler { get; set; }
    }

    public class TMPDialogSettings
    {
        public TMPDialogSettings()
        {
            AffirmativeButtonText = "OK";
            NegativeButtonText = "Отменить";

            ColorScheme = TMPDialogColorScheme.Theme;
            AnimateShow = AnimateHide = true;

            MaximumBodyHeight = Double.NaN;

            DefaultText = "";
        }

        /// <summary>
        /// Возвращает/задает текст кнопки, используемой для подтверждения действия. Для примера: "OK" или "Да"
        /// </summary>
        public string AffirmativeButtonText { get; set; }

        /// <summary>
        /// Возвращает/задает текст кнопки, используемой для отмены действия. Для примера: "Отменить" или "Нет".
        /// </summary>
        public string NegativeButtonText { get; set; }

        /// <summary>
        /// Возвращает/задает текст первой вспомогательной кнопки
        /// </summary>
        public string FirstAuxiliaryButtonText { get; set; }

        /// <summary>
        /// Возвращает/задает текст второй вспомогательной кнопки
        /// </summary>
        public string SecondAuxiliaryButtonText { get; set; }

        /// <summary>
        /// Возвращает/задает цветовую схему
        /// </summary>
        public TMPDialogColorScheme ColorScheme { get; set; }

        /// <summary>
        /// Разрешает/запрещает отображение анимации при появлении диалога
        /// </summary>
        public bool AnimateShow { get; set; }

        /// <summary>
        /// Разрешает/запрещает отображение анимации при закрытии диалога
        /// </summary>
        public bool AnimateHide { get; set; }

        /// <summary>
        /// Возвращает/задает вспомогательный текст (для InputDialog)
        /// </summary>
        ///
        public string DefaultText { get; set; }

        /// <summary>
        /// Возвращает/задает максимальную высоту. (По умолчанию не лимитирована, <a href="http://msdn.microsoft.com/de-de/library/system.double.nan">Double.NaN</a>)
        /// </summary>
        public double MaximumBodyHeight { get; set; }
    }

    /// <summary>
    /// Перечисление, представляющее вариант цветовой схемы для диологов
    /// </summary>
    public enum TMPDialogColorScheme
    {
        Theme = 0,
        Accented = 1,
        Inverted = 2
    }
}