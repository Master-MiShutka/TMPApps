using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using TMPApplication.WpfDialogs.Contracts;

namespace TMPApplication.WpfDialogs
{
	class MessageDialog : DialogBase, IMessageDialog
	{
		public MessageDialog(
			IDialogHost dialogHost, 
			DialogMode dialogMode,
			string message,
			Dispatcher dispatcher,
            System.Windows.MessageBoxImage image = MessageBoxImage.None)
			: base(dialogHost, dialogMode, dispatcher, image)
		{
			InvokeUICall(() =>
				{
					_messageTextBlock = new TextBlock
					{
						Text = message,
                        TextAlignment = TextAlignment.Center,
						HorizontalAlignment = HorizontalAlignment.Center,
						VerticalAlignment = VerticalAlignment.Center,
						TextWrapping = TextWrapping.Wrap,
					};
					SetContent(_messageTextBlock);
				});
		}

		private TextBlock _messageTextBlock;

		#region Implementation of IMessageDialog

		public string Message
		{
			get
			{
				var text = string.Empty;
				InvokeUICall(
					() => text = _messageTextBlock.Text);
				return text;
			}
			set
			{
				InvokeUICall(
					() => _messageTextBlock.Text = value);
			}
		}

		#endregion
	}
}