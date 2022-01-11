namespace TMPApplication
{
    using System;
    using System.Windows;
    using System.Windows.Input;

    public class CloseThisWindowCommand : ICommand
    {
        #region ICommand Members

        public bool CanExecute(object parameter)
        {
            // we can only close Windows
            return parameter is Window;
        }

#pragma warning disable CS0067 // The event 'CloseThisWindowCommand.CanExecuteChanged' is never used
        public event EventHandler CanExecuteChanged;
#pragma warning restore CS0067 // The event 'CloseThisWindowCommand.CanExecuteChanged' is never used

        public void Execute(object parameter)
        {
            if (this.CanExecute(parameter))
            {
                ((Window)parameter).Close();
            }
        }

        #endregion

        private CloseThisWindowCommand()
        {
        }

        public static readonly ICommand Instance = new CloseThisWindowCommand();
    }
}
