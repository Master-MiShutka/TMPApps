using TMP.DWRES.Graph;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using Model = TMP.DWRES.Objects;

using TMP.DWRES.DB;

namespace TMP.DWRES.ViewModel
{
    public partial class MainWindowViewModel
    {
        #region Commands

        public System.Windows.Input.ICommand CommandLoadUnlodDB
        {
            get
            {
                return new GUI.DelegateCommand(() =>
                {
                    SelectDBAndConnect();
                });
            }
        }

        public ICommand CommandLoadDBFromFile
        {
            get
            {
                return new GUI.DelegateCommand(() =>
                {
                    DBFileName = string.Empty;
                    SelectDBAndConnect();
                });
            }
        }

        public ICommand CommandShowAbout
        {
            get
            {
                return new GUI.DelegateCommand(() =>
                {
                    GUI.AboutWindow about = new GUI.AboutWindow();
                    if (this.MainWindow == null)
                        about.Owner = Application.Current.MainWindow;
                    else
                        about.Owner = this.MainWindow;
                    about.ShowDialog();
                });
            }
        }

        public ICommand CommandIncreaseFontSize { get { return new GUI.DelegateCommand<Window>((w) => w.FontSize++); } }
        public ICommand CommandDecreaseFontSize { get { return new GUI.DelegateCommand<Window>((w) => w.FontSize--); } }
        public ICommand CommandClose { get { return new GUI.DelegateCommand<Window>((w) => w.Close()); } }

        public ICommand CommandConnectToServer
        {
            get
            {
                return new GUI.DelegateCommand(() =>
                {
                    Window w = null;
                    if (this.MainWindow == null)
                       w = Application.Current.MainWindow;
                    else
                        w = this.MainWindow;

                    GUI.ConnectToFirebirdServerWindow cw = new GUI.ConnectToFirebirdServerWindow(w);
                    _dbConnectionParams = cw.Show();
                    if (_dbConnectionParams == null)
                        return;
                    //Process(true);
                });
            }
        }

        public ICommand CommandShowFiderSchemeTable { get { return new GUI.DelegateCommand(() => ShowFiderSchemeTable()); } }
        public ICommand CommandShowSubstationScheme { get { return new GUI.DelegateCommand(() => BuildSubstationGraph()); } }

        public ICommand CommandRefreshGraph { get { return new GUI.DelegateCommand(() => BuildGraph());} }

        private ICommand _commandRelayoutGraph;
        public ICommand CommandRelayoutGraph { get { return _commandRelayoutGraph; } set { _commandRelayoutGraph = value; RaisePropertyChanged("CommandRelayoutGraph"); } }
        
        private ICommand _commandSelectLayoutAlgorithmType;
        public ICommand CommandSelectLayoutAlgorithmType { get { return _commandSelectLayoutAlgorithmType; } set { _commandSelectLayoutAlgorithmType = value; RaisePropertyChanged("CommandSelectLayoutAlgorithmType"); } }
        

        private ICommand _commandCopyGraph;
        public ICommand CommandCopyGraph { get { return _commandCopyGraph; } set { _commandCopyGraph = value; RaisePropertyChanged("CommandCopyGraph"); } }

        private ICommand _commandSaveGraph;
        public ICommand CommandSaveGraph { get { return _commandSaveGraph; } set { _commandSaveGraph = value; RaisePropertyChanged("CommandSaveGraph"); } }
        
        private ICommand _commandPrintGraph;
        public ICommand CommandPrintGraph { get { return _commandPrintGraph; } set { _commandPrintGraph = value; RaisePropertyChanged("CommandPrintGraph"); } }

        #endregion
    }
}
