using System.Windows;

namespace WpfDialogManagement
{
	interface IDialogHost
	{
		void ShowDialog(DialogBaseControl dialog);
		void HideDialog(DialogBaseControl dialog);
		FrameworkElement GetCurrentContent();
	}
}