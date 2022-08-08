namespace WindowWithDialogs.WpfDialogs
{
    using System.Windows;
    using System.Windows.Controls;

    internal class DialogLayeringHelper : IDialogHost
    {
        public DialogLayeringHelper(object parent, UIElement uIElement)
        {
            this.dialogContainer = (Grid)parent;
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