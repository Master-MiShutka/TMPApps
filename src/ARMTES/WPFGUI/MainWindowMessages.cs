using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TMP.Wpf.CommonControls.Dialogs;

namespace TMP.ARMTES
{
    public partial class MainWindow
    {
        private const string defaultAppStatus = "Ожидание команды";

        private ProgressDialogController progressController;

        private async Task<bool> ShowProgress(string message)
        {
            statusBarAppStatus.Content = message;
            if (progressController != null)
                if (progressController.IsOpen)
                    progressController.SetMessage(message);
                else
                    progressController = await this.ShowProgressAsync(MainViewModel.Instance.Title, message, true, new TMPDialogSettings() { AnimateHide = false, AnimateShow = false });
            return true;
        }

        private async Task<bool> ShowProgress(string title, string message, bool isCancelable = false, TMPDialogSettings settings = null)
        {
            statusBarAppStatus.Content = message;

            await CloseProgress();

            if (settings == null)
                settings = new TMPDialogSettings() { AnimateHide = false, AnimateShow = false };

            if (progressController != null)
            {
                if (progressController.IsOpen == true)
                    progressController.SetMessage(message);
                else
                    progressController = await this.ShowProgressAsync(MainViewModel.Instance.Title, message, isCancelable, settings);
            }
            else
            {
                progressController = await this.ShowProgressAsync(title, message, isCancelable, settings);
                progressController.SetIndeterminate();
            }
            return true;
        }

        private async void SetAppStatus(string status = defaultAppStatus)
        {
            this.Dispatcher.Invoke(new Action(() => statusBarAppStatus.Content = status));
            
            if (status == defaultAppStatus)
                await CloseProgress();
        }

        private async Task<bool> CloseProgress()
        {
            if (progressController != null)
                if (progressController.IsOpen)
                    await progressController.CloseAsync();
            return true;
        }

        private async Task<MessageDialogResult> ShowErrorMessageAsync(string message)
        {
            SetAppStatus();

            return await this.ShowMessage("ОШИБКА", message,
                new TMPDialogSettings() { ColorScheme = TMPDialogColorScheme.Accented, DefaultText = "Просто теекст!" });
        }
        private void ShowErrorMessage(string message)
        {
            SetAppStatus();

            this.ShowMessage("ОШИБКА", message,
                new TMPDialogSettings() { ColorScheme = TMPDialogColorScheme.Accented, DefaultText = "Просто теекст!" });
        }

        private void DebugPrint(string message)
        {
            System.Diagnostics.Debug.WriteLine(message,
                            MainViewModel.Instance.Title);
        }
        private void DebugPrint(string format, params object[] args)
        {
            System.Diagnostics.Debug.WriteLine(String.Format(format, args), 
                            MainViewModel.Instance.Title);
        }
    }
}
