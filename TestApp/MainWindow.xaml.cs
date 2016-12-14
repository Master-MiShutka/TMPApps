using System;
using System.Threading.Tasks;
using System.Windows;
using TMP.Wpf.CommonControls.Dialogs;

namespace TestApp
{
    using TMP.Wpf.CommonControls;
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : TMPWindow
    {
        private readonly MainWindowViewModel _viewModel;
        private bool _shutdown;

        public MainWindow()
        {
            _viewModel = new MainWindowViewModel();
            DataContext = _viewModel;

            InitializeComponent();
        }

        private async void ShowInputDialog(object sender, RoutedEventArgs e)
        {
            this.TMPDialogOptions.ColorScheme = UseAccentForDialogsMenuItem.IsChecked ? TMPDialogColorScheme.Accented : TMPDialogColorScheme.Theme;

            var result = await this.ShowInputAsync("Hello!", "What is your name?");

            if (result == null) //user pressed cancel
                return;

            await this.ShowMessageAsync("Hello", "Hello " + result + "!");
        }

        private async void ShowLoginDialog(object sender, RoutedEventArgs e)
        {
            this.TMPDialogOptions.ColorScheme = UseAccentForDialogsMenuItem.IsChecked ? TMPDialogColorScheme.Accented : TMPDialogColorScheme.Theme;
            LoginDialogData result = await this.ShowLoginAsync("Authentication", "Enter your credentials", new LoginDialogSettings { ColorScheme = this.TMPDialogOptions.ColorScheme, InitialUsername = "MahApps" });
            if (result == null)
            {
                //User pressed cancel
            }
            else
            {
                MessageDialogResult messageResult = await this.ShowMessageAsync("Authentication Information", String.Format("Username: {0}\nPassword: {1}", result.Username, result.Password));
            }
        }

        private async void ShowMessageDialog(object sender, RoutedEventArgs e)
        {
            // This demo runs on .Net 4.0, but we're using the Microsoft.Bcl.Async package so we have async/await support
            // The package is only used by the demo and not a dependency of the library!
            TMPDialogOptions.ColorScheme = UseAccentForDialogsMenuItem.IsChecked ? TMPDialogColorScheme.Accented : TMPDialogColorScheme.Theme;

            var mySettings = new TMPDialogSettings()
            {
                AffirmativeButtonText = "Hi",
                NegativeButtonText = "Go away!",
                FirstAuxiliaryButtonText = "Cancel",
                ColorScheme = UseAccentForDialogsMenuItem.IsChecked ? TMPDialogColorScheme.Accented : TMPDialogColorScheme.Theme
            };

            MessageDialogResult result = await this.ShowMessageAsync("Hello!", "Welcome to the world of metro!",
                MessageDialogStyle.AffirmativeAndNegativeAndSingleAuxiliary, mySettings);

            if (result != MessageDialogResult.FirstAuxiliary)
                await this.ShowMessageAsync("Result", "You said: " + (result == MessageDialogResult.Affirmative ? mySettings.AffirmativeButtonText : mySettings.NegativeButtonText +
                    Environment.NewLine + Environment.NewLine + "This dialog will follow the Use Accent setting."));
        }

        private async void ShowLimitedMessageDialog(object sender, RoutedEventArgs e)
        {
            var mySettings = new TMPDialogSettings()
            {
                AffirmativeButtonText = "Hi",
                NegativeButtonText = "Go away!",
                FirstAuxiliaryButtonText = "Cancel",
                MaximumBodyHeight = 100,
                ColorScheme = UseAccentForDialogsMenuItem.IsChecked ? TMPDialogColorScheme.Accented : TMPDialogColorScheme.Theme
            };

            MessageDialogResult result = await this.ShowMessageAsync("Hello!", "Welcome to the world of metro!" + string.Join(Environment.NewLine, "abc", "def", "ghi", "jkl", "mno", "pqr", "stu", "vwx", "yz"),
                MessageDialogStyle.AffirmativeAndNegativeAndSingleAuxiliary, mySettings);

            if (result != MessageDialogResult.FirstAuxiliary)
                await this.ShowMessageAsync("Result", "You said: " + (result == MessageDialogResult.Affirmative ? mySettings.AffirmativeButtonText : mySettings.NegativeButtonText +
                    Environment.NewLine + Environment.NewLine + "This dialog will follow the Use Accent setting."));
        }

        private async void ShowProgressDialog(object sender, RoutedEventArgs e)
        {
            this.TMPDialogOptions.ColorScheme = UseAccentForDialogsMenuItem.IsChecked ? TMPDialogColorScheme.Accented : TMPDialogColorScheme.Theme;

            var controller = await this.ShowProgressAsync("Please wait...", "We are baking some cupcakes!");

            await Task.Delay(5000);

            controller.SetCancelable(true);

            double i = 0.0;
            while (i < 6.0)
            {
                double val = (i / 100.0) * 20.0;
                controller.SetProgress(val);
                controller.SetMessage("Baking cupcake: " + i + "...");

                if (controller.IsCanceled)
                    break; //canceled progressdialog auto closes.

                i += 1.0;

                await Task.Delay(2000);
            }

            await controller.CloseAsync();

            if (controller.IsCanceled)
            {
                await this.ShowMessageAsync("No cupcakes!", "You stopped baking!");
            }
            else
            {
                await this.ShowMessageAsync("Cupcakes!", "Your cupcakes are finished! Enjoy!");
            }
        }

        private async void TMPWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = !_shutdown && _viewModel.QuitConfirmationEnabled;
            if (_shutdown) return;

            var mySettings = new TMPDialogSettings()
            {
                AffirmativeButtonText = "Quit",
                NegativeButtonText = "Cancel",
                AnimateShow = true,
                AnimateHide = false
            };

            MessageDialogResult result = await this.ShowMessageAsync("Quit application?",
                "Sure you want to quit application?",
                MessageDialogStyle.AffirmativeAndNegative, mySettings);

            _shutdown = result == MessageDialogResult.Affirmative;

            if (_shutdown)
                Application.Current.Shutdown();
        }
    }
}