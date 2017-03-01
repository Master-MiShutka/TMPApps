using System.Windows.Threading;
using TMPApplication.WpfDialogs.Contracts;

namespace TMPApplication.WpfDialogs
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