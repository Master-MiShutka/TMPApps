namespace WindowWithDialogs
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;

    internal class DialogLayeringHelper : IDialogHost
    {
        public DialogLayeringHelper(Grid parent, UIElement uIElement)
        {
            this.dialogContainer = parent;
            this.uIElement = uIElement;
        }

        private Grid dialogContainer;
        private UIElement uIElement;

        #region Implementation of IDialogHost

        public void ShowDialog(DialogBaseControl dialog)
        {
            if (this.uIElement.IsEnabled)
            {
                this.uIElement.IsEnabled = false;
            }

            foreach (UIElement child in this.dialogContainer.Children)
            {
                child.IsEnabled = false;
            }

            this.dialogContainer.Children.Add(dialog);
        }

        public void HideDialog(DialogBaseControl dialog)
        {
            this.dialogContainer.Children.Remove(dialog);

            if (this.dialogContainer.Children.Count > 0)
            {
                this.dialogContainer.Children[this.dialogContainer.Children.Count - 1].IsEnabled = true;
            }

            if (this.dialogContainer.Children.Count == 0)
            {
                this.uIElement.IsEnabled = true;
            }
        }

        public FrameworkElement GetCurrentContent()
        {
            return this.dialogContainer;
        }

        #endregion
    }
}