using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace TMPApplication.WpfDialogs
{
	class DialogLayeringHelper : IDialogHost
	{
        public DialogLayeringHelper(Grid parent)
		{
			_dialogContainer = parent;
		}
        private Grid _dialogContainer;

		#region Implementation of IDialogHost

		public void ShowDialog(DialogBaseControl dialog)
		{
            _dialogContainer.Children.Add(dialog);
        }

		public void HideDialog(DialogBaseControl dialog)
		{
            _dialogContainer.Children.Remove(dialog);
        }

		public FrameworkElement GetCurrentContent()
		{
			return _dialogContainer;
		}

		#endregion
	}
}