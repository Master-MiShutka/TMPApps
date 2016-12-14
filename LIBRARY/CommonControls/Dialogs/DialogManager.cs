using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using TMP.Wpf.CommonControls.Behaviors;

namespace TMP.Wpf.CommonControls.Dialogs
{
    public static class DialogManager
    {
        /// <summary>
        /// Создает LoginDialog внутри текущего окна
        /// </summary>
        /// <param name="window">Окно-владелец диалога</param>
        /// <param name="title">Заголовок LoginDialog</param>
        /// <param name="message">Сообщение в диалоге</param>
        /// <param name="settings">Дополнительные параметры, переопределяющие глабальные параметры диалога</param>
        /// <returns>Введенный текст или null, если пользователь отменил операцию</returns>
        public static Task<LoginDialogData> ShowLoginAsync(this TMPWindow window, string title, string message, LoginDialogSettings settings = null)
        {
            window.Dispatcher.VerifyAccess();
            return HandleOverlayOnShow(settings, window).ContinueWith(z =>
            {
                return (Task<LoginDialogData>)window.Dispatcher.Invoke(new Func<Task<LoginDialogData>>(() =>
                {
                    if (settings == null)
                    {
                        settings = new LoginDialogSettings();
                    }

                    //create the dialog control
                    LoginDialog dialog = new LoginDialog(window, settings)
                    {
                        Title = title,
                        Message = message
                    };

                    SizeChangedEventHandler sizeHandler = SetupAndOpenDialog(window, dialog);
                    dialog.SizeChangedHandler = sizeHandler;

                    return dialog.WaitForLoadAsync().ContinueWith(x =>
                    {
                        if (DialogOpened != null)
                        {
                            window.Dispatcher.BeginInvoke(new Action(() => DialogOpened(window, new DialogStateChangedEventArgs())));
                        }

                        return dialog.WaitForButtonPressAsync().ContinueWith(y =>
                        {
                            //once a button as been clicked, begin removing the dialog.

                            dialog.OnClose();

                            if (DialogClosed != null)
                            {
                                window.Dispatcher.BeginInvoke(new Action(() => DialogClosed(window, new DialogStateChangedEventArgs())));
                            }

                            Task closingTask = (Task)window.Dispatcher.Invoke(new Func<Task>(() => dialog._WaitForCloseAsync()));
                            return closingTask.ContinueWith(a =>
                            {
                                return ((Task)window.Dispatcher.Invoke(new Func<Task>(() =>
                                {
                                    window.SizeChanged -= sizeHandler;

                                    window.tmpDialogContainer.Children.Remove(dialog); //remove the dialog from the container

                                    return HandleOverlayOnHide(settings, window);
                                    //window.overlayBox.Visibility = System.Windows.Visibility.Hidden; //deactive the overlay effect
                                }))).ContinueWith(y3 => y).Unwrap();
                            });
                        }).Unwrap();
                    }).Unwrap().Unwrap();
                }));
            }).Unwrap();
        }

        /// <summary>
        /// Creates a InputDialog inside of the current window.
        /// </summary>
        /// <param name="window">The TMPWindow</param>
        /// <param name="title">The title of the MessageDialog.</param>
        /// <param name="message">The message contained within the MessageDialog.</param>
        /// <param name="settings">Optional settings that override the global tmp dialog settings.</param>
        /// <returns>The text that was entered or null (Nothing in Visual Basic) if the user cancelled the operation.</returns>
        public static Task<string> ShowInputAsync(this TMPWindow window, string title, string message, TMPDialogSettings settings = null)
        {
            window.Dispatcher.VerifyAccess();
            return HandleOverlayOnShow(settings, window).ContinueWith(z =>
            {
                return (Task<string>)window.Dispatcher.Invoke(new Func<Task<string>>(() =>
                {
                    if (settings == null)
                        settings = window.TMPDialogOptions;

                    //create the dialog control
                    var dialog = new InputDialog(window, settings)
                    {
                        Title = title,
                        Message = message,
                        Input = settings.DefaultText
                    };

                    SizeChangedEventHandler sizeHandler = SetupAndOpenDialog(window, dialog);
                    dialog.SizeChangedHandler = sizeHandler;

                    return dialog.WaitForLoadAsync().ContinueWith(x =>
                    {
                        if (DialogOpened != null)
                        {
                            window.Dispatcher.BeginInvoke(new Action(() => DialogOpened(window, new DialogStateChangedEventArgs())));
                        }

                        return dialog.WaitForButtonPressAsync().ContinueWith(y =>
                        {
                            //once a button as been clicked, begin removing the dialog.

                            dialog.OnClose();

                            if (DialogClosed != null)
                            {
                                window.Dispatcher.BeginInvoke(new Action(() => DialogClosed(window, new DialogStateChangedEventArgs())));
                            }

                            Task closingTask = (Task)window.Dispatcher.Invoke(new Func<Task>(() => dialog._WaitForCloseAsync()));
                            return closingTask.ContinueWith(a =>
                            {
                                return ((Task)window.Dispatcher.Invoke(new Func<Task>(() =>
                                {
                                    window.SizeChanged -= sizeHandler;

                                    window.tmpDialogContainer.Children.Remove(dialog); //remove the dialog from the container

                                    return HandleOverlayOnHide(settings, window);
                                }))).ContinueWith(y3 => y).Unwrap();
                            });
                        }).Unwrap();
                    }).Unwrap().Unwrap();
                }));
            }).Unwrap();
        }

        /// <summary>
        /// Creates a MessageDialog inside of the current window.
        /// </summary>
        /// <param name="window">The TMPWindow</param>
        /// <param name="title">The title of the MessageDialog.</param>
        /// <param name="message">The message contained within the MessageDialog.</param>
        /// <param name="style">The type of buttons to use.</param>
        /// <param name="settings">Optional settings that override the global tmp dialog settings.</param>
        /// <returns>A task promising the result of which button was pressed.</returns>
        public static Task<MessageDialogResult> ShowMessageAsync(this TMPWindow window, string title, string message, MessageDialogStyle style = MessageDialogStyle.Affirmative, TMPDialogSettings settings = null)
        {
            window.Dispatcher.VerifyAccess();
            return HandleOverlayOnShow(settings, window).ContinueWith(z =>
            {
                return (Task<MessageDialogResult>)window.Dispatcher.Invoke(new Func<Task<MessageDialogResult>>(() =>
                {
                    if (settings == null)
                    {
                        settings = window.TMPDialogOptions;
                    }

                    //create the dialog control
                    var dialog = new MessageDialog(window, settings)
                    {
                        Message = message,
                        Title = title,
                        ButtonStyle = style
                    };

                    SizeChangedEventHandler sizeHandler = SetupAndOpenDialog(window, dialog);
                    dialog.SizeChangedHandler = sizeHandler;

                    return dialog.WaitForLoadAsync().ContinueWith(x =>
                    {
                        if (DialogOpened != null)
                        {
                            window.Dispatcher.BeginInvoke(new Action(() => DialogOpened(window, new DialogStateChangedEventArgs())));
                        }

                        return dialog.WaitForButtonPressAsync().ContinueWith(y =>
                        {
                            //once a button as been clicked, begin removing the dialog.

                            dialog.OnClose();

                            if (DialogClosed != null)
                            {
                                window.Dispatcher.BeginInvoke(new Action(() => DialogClosed(window, new DialogStateChangedEventArgs())));
                            }

                            Task closingTask = (Task)window.Dispatcher.Invoke(new Func<Task>(() => dialog._WaitForCloseAsync()));
                            return closingTask.ContinueWith(a =>
                            {
                                return ((Task)window.Dispatcher.Invoke(new Func<Task>(() =>
                                {
                                    window.SizeChanged -= sizeHandler;

                                    window.tmpDialogContainer.Children.Remove(dialog); //remove the dialog from the container

                                    return HandleOverlayOnHide(settings, window);
                                }))).ContinueWith(y3 => y).Unwrap();
                            });
                        }).Unwrap();
                    }).Unwrap().Unwrap();
                }));
            }).Unwrap();
        }

        /// <summary>
        /// Creates a ProgressDialog inside of the current window.
        /// </summary>
        /// <param name="window">The TMPWindow</param>
        /// <param name="title">The title of the ProgressDialog.</param>
        /// <param name="message">The message within the ProgressDialog.</param>
        /// <param name="isCancelable">Determines if the cancel button is visible.</param>
        /// <param name="settings">Optional Settings that override the global tmp dialog settings.</param>
        /// <returns>A task promising the instance of ProgressDialogController for this operation.</returns>
        public static Task<ProgressDialogController> ShowProgressAsync(this TMPWindow window, string title, string message, bool isCancelable = false, TMPDialogSettings settings = null)
        {
            window.Dispatcher.VerifyAccess();

            return HandleOverlayOnShow(settings, window).ContinueWith(z =>
            {
                return ((Task<ProgressDialogController>)window.Dispatcher.Invoke(new Func<Task<ProgressDialogController>>(() =>
                {
                    var dialog = new ProgressDialog(window)
                    {
                        Message = message,
                        Title = title,
                        IsCancelable = isCancelable
                    };

                    if (settings == null)
                    {
                        settings = window.TMPDialogOptions;
                    }

                    dialog.NegativeButtonText = settings.NegativeButtonText;

                    SizeChangedEventHandler sizeHandler = SetupAndOpenDialog(window, dialog);
                    dialog.SizeChangedHandler = sizeHandler;

                    return dialog.WaitForLoadAsync().ContinueWith(x =>
                    {
                        if (DialogOpened != null)
                        {
                            window.Dispatcher.BeginInvoke(new Action(() => DialogOpened(window, new DialogStateChangedEventArgs())));
                        }

                        return new ProgressDialogController(dialog, () =>
                        {
                            dialog.OnClose();

                            if (DialogClosed != null)
                            {
                                window.Dispatcher.BeginInvoke(new Action(() => DialogClosed(window, new DialogStateChangedEventArgs())));
                            }

                            Task closingTask = (Task)window.Dispatcher.Invoke(new Func<Task>(() => dialog._WaitForCloseAsync()));
                            return closingTask.ContinueWith(a =>
                            {
                                return (Task)window.Dispatcher.Invoke(new Func<Task>(() =>
                                {
                                    window.SizeChanged -= sizeHandler;

                                    window.tmpDialogContainer.Children.Remove(dialog); //remove the dialog from the container

                                    return HandleOverlayOnHide(settings, window);
                                }));
                            }).Unwrap();
                        });
                    });
                })));
            }).Unwrap();
        }

        private static Task HandleOverlayOnHide(TMPDialogSettings settings, TMPWindow window)
        {
            return (settings == null ? window.HideOverlayAsync() : Task.Factory.StartNew(() => window.Dispatcher.Invoke(new Action(window.HideOverlay))));
        }

        private static Task HandleOverlayOnShow(TMPDialogSettings settings, TMPWindow window)
        {
            return (settings == null ? window.ShowOverlayAsync() : Task.Factory.StartNew(() => window.Dispatcher.Invoke(new Action(window.ShowOverlay))));
        }

        /// <summary>
        /// Adds a TMP Dialog instance to the specified window and makes it visible.
        /// <para>Note that this method returns as soon as the dialog is loaded and won't wait on a call of <see cref="HideTMPDialogAsync"/>.</para>
        /// <para>You can still close the resulting dialog with <see cref="HideTMPDialogAsync"/>.</para>
        /// </summary>
        /// <param name="window">The owning window of the dialog.</param>
        /// <param name="dialog">The dialog instance itself.</param>
        /// <returns>A task representing the operation.</returns>
        /// <exception cref="InvalidOperationException">The <paramref name="dialog"/> is already visible in the window.</exception>
        public static Task ShowTMPDialogAsync(this TMPWindow window, BaseTMPDialog dialog, TMPDialogSettings settings = null)
        {
            window.Dispatcher.VerifyAccess();
            if (window.tmpDialogContainer.Children.Contains(dialog))
                throw new InvalidOperationException("The provided dialog is already visible in the specified window.");

            return HandleOverlayOnShow(settings, window).ContinueWith(z =>
            {
                dialog.Dispatcher.Invoke(new Action(() =>
                {
                    SizeChangedEventHandler sizeHandler = SetupAndOpenDialog(window, dialog);
                    dialog.SizeChangedHandler = sizeHandler;
                }));
            }).ContinueWith(y =>
                ((Task)dialog.Dispatcher.Invoke(new Func<Task>(() => dialog.WaitForLoadAsync().ContinueWith(x =>
                {
                    dialog.OnShown();

                    if (DialogOpened != null)
                    {
                        DialogOpened(window, new DialogStateChangedEventArgs());
                    }
                })))));
        }

        /// <summary>
        /// Hides a visible TMP Dialog instance.
        /// </summary>
        /// <param name="window">The window with the dialog that is visible.</param>
        /// <param name="dialog">The dialog instance to hide.</param>
        /// <returns>A task representing the operation.</returns>
        /// <exception cref="InvalidOperationException">
        /// The <paramref name="dialog"/> is not visible in the window.
        /// This happens if <see cref="ShowTMPDialogAsync"/> hasn't been called before.
        /// </exception>
        public static Task HideTMPDialogAsync(this TMPWindow window, BaseTMPDialog dialog, TMPDialogSettings settings = null)
        {
            window.Dispatcher.VerifyAccess();
            if (!window.tmpDialogContainer.Children.Contains(dialog))
                throw new InvalidOperationException("The provided dialog is not visible in the specified window.");

            window.SizeChanged -= dialog.SizeChangedHandler;

            dialog.OnClose();

            Task closingTask = (Task)window.Dispatcher.Invoke(new Func<Task>(dialog._WaitForCloseAsync));
            return closingTask.ContinueWith(a =>
            {
                if (DialogClosed != null)
                {
                    window.Dispatcher.BeginInvoke(new Action(() => DialogClosed(window, new DialogStateChangedEventArgs())));
                }

                return (Task)window.Dispatcher.Invoke(new Func<Task>(() =>
                {
                    window.tmpDialogContainer.Children.Remove(dialog); //remove the dialog from the container

                    return HandleOverlayOnHide(settings, window);
                }));
            }).Unwrap();
        }

        /// <summary>
        /// Gets the current shown dialog.
        /// </summary>
        /// <param name="window">The dialog owner.</param>
        public static Task<TDialog> GetCurrentDialogAsync<TDialog>(this TMPWindow window) where TDialog : BaseTMPDialog
        {
            window.Dispatcher.VerifyAccess();
            var t = new TaskCompletionSource<TDialog>();
            window.Dispatcher.Invoke((Action)(() =>
            {
                TDialog dialog = window.tmpDialogContainer.Children.OfType<TDialog>().LastOrDefault();
                t.TrySetResult(dialog);
            }));
            return t.Task;
        }

        private static SizeChangedEventHandler SetupAndOpenDialog(TMPWindow window, BaseTMPDialog dialog)
        {
            dialog.SetValue(Panel.ZIndexProperty, (int)window.overlayBox.GetValue(Panel.ZIndexProperty) + 1);
            dialog.MinHeight = window.ActualHeight / 4.0;
            dialog.MaxHeight = window.ActualHeight;

            SizeChangedEventHandler sizeHandler = (sender, args) =>
            {
                dialog.MinHeight = window.ActualHeight / 4.0;
                dialog.MaxHeight = window.ActualHeight;
            };

            window.SizeChanged += sizeHandler;

            window.tmpDialogContainer.Children.Add(dialog); //add the dialog to the container

            dialog.OnShown();

            return sizeHandler;
        }

        public static BaseTMPDialog ShowDialogExternally(this BaseTMPDialog dialog)
        {
            Window win = SetupExternalDialogWindow(dialog);

            dialog.OnShown();
            win.Show();

            return dialog;
        }

        public static BaseTMPDialog ShowModalDialogExternally(this BaseTMPDialog dialog)
        {
            Window win = SetupExternalDialogWindow(dialog);

            dialog.OnShown();
            win.ShowDialog();

            return dialog;
        }

        private static Window SetupExternalDialogWindow(BaseTMPDialog dialog)
        {
            var win = new TMPWindow
            {
                ShowInTaskbar = false,
                ShowActivated = true,
                Topmost = true,
                ResizeMode = ResizeMode.NoResize,
                WindowStyle = WindowStyle.None,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                ShowTitleBar = false,
                ShowCloseButton = false,
                Background = dialog.Background
            };

            try
            {
                win.GlowBrush = win.TryFindResource("AccentColorBrush") as SolidColorBrush;
            }
            catch (Exception) { }

            win.Width = SystemParameters.PrimaryScreenWidth;
            win.MinHeight = SystemParameters.PrimaryScreenHeight / 4.0;
            win.SizeToContent = SizeToContent.Height;

            var glowWindow = new GlowWindowBehavior();
            glowWindow.Attach(win);

            dialog.ParentDialogWindow = win; //THIS IS ONLY, I REPEAT, ONLY SET FOR EXTERNAL DIALOGS!

            win.Content = dialog;

            EventHandler closedHandler = null;
            closedHandler = (sender, args) =>
            {
                win.Closed -= closedHandler;
                dialog.ParentDialogWindow = null;
                win.Content = null;
            };
            win.Closed += closedHandler;

            return win;
        }

        public static event EventHandler<DialogStateChangedEventArgs> DialogOpened;

        public static event EventHandler<DialogStateChangedEventArgs> DialogClosed;
    }
}