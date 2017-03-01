using System.Windows;

namespace TMPApplication.WpfDialogs
{
	interface IDialogHost
	{
		void ShowDialog(DialogBaseControl dialog);
		void HideDialog(DialogBaseControl dialog);
		FrameworkElement GetCurrentContent();
	}
}