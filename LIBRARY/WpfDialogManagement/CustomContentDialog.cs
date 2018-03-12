using System.Windows.Threading;
using WpfDialogManagement.Contracts;

namespace WpfDialogManagement
{
	class CustomContentDialog : DialogBase, ICustomContentDialog
	{
		public CustomContentDialog(
			IDialogHost dialogHost, 
			DialogMode dialogMode,
			object content,
			Dispatcher dispatcher)
			: base(dialogHost, dialogMode, dispatcher)
		{
			SetContent(content);
		}
	}
}