namespace TMP.PrintEngine.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing.Printing;
    using System.Linq;
    using System.Printing;
    using System.Printing.Interop;
    using System.Runtime.InteropServices;
    using System.Timers;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Documents;
    using System.Windows.Forms;
    using System.Windows.Input;
    using System.Windows.Interop;
    using System.Windows.Media;
    using System.Windows.Shapes;
    using System.Windows.Threading;
    using TMP.PrintEngine.Controls.ProgressDialog;
    using TMP.PrintEngine.Controls.WaitScreen;
    using TMP.PrintEngine.Extensions;
    using TMP.PrintEngine.Utils;
    using TMP.PrintEngine.Views;
    using Application = System.Windows.Application;
    using HorizontalAlignment = System.Windows.HorizontalAlignment;
    using Orientation = System.Windows.Controls.Orientation;

    public abstract class APrintControlViewModel : AViewModel, IViewModel
    {
        protected DocumentPaginator Paginator;

        protected IWaitScreenViewModel WaitScreen { get; set; }

        protected IProgressDialogViewModel ProgressDialog { get; set; }

        public ICommand ChangePaperCommand { get; set; }

        protected bool ScaleCanceling;

        public double OldScale { get; set; }

        [DllImport("winspool.Drv", EntryPoint = "DocumentPropertiesW", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        private static extern int DocumentProperties(IntPtr hwnd, IntPtr hPrinter, [MarshalAs(UnmanagedType.LPWStr)] string pDeviceName, IntPtr pDevModeOutput, IntPtr pDevModeInput, int fMode);

        #region Dependency Properties
        public static readonly DependencyProperty IsPotraitProperty = DependencyProperty.Register(
            "IsPotrait",
            typeof(bool),
            typeof(APrintControlViewModel),
            new FrameworkPropertyMetadata(false, new PropertyChangedCallback(OnDependencyPropertyChanged)));

        public static readonly DependencyProperty IsLandscapeProperty = DependencyProperty.Register(
            "IsLandscape",
            typeof(bool),
            typeof(APrintControlViewModel),
            new FrameworkPropertyMetadata(false, new PropertyChangedCallback(OnDependencyPropertyChanged)));

        public static readonly DependencyProperty IsAdvancedPrintingOptionOpenProperty = DependencyProperty.Register(
            "IsAdvancedPrintingOptionOpen",
            typeof(bool),
            typeof(APrintControlViewModel),
            new FrameworkPropertyMetadata(false, new PropertyChangedCallback(OnDependencyPropertyChanged)));

        public static readonly DependencyProperty IsMarkPageNumbersProperty = DependencyProperty.Register(
            "IsMarkPageNumbers",
            typeof(bool),
            typeof(APrintControlViewModel),
            new FrameworkPropertyMetadata(new PropertyChangedCallback(OnDependencyPropertyChanged)));

        public static readonly DependencyProperty PageOrientationProperty = DependencyProperty.Register(
            "PageOrientation",
            typeof(PageOrientation),
            typeof(APrintControlViewModel),
            new FrameworkPropertyMetadata(PageOrientation.Portrait, new PropertyChangedCallback(OnDependencyPropertyChanged)));

        public static readonly DependencyProperty PageOrientationStringProperty = DependencyProperty.Register(
            "PageOrientationString",
            typeof(string),
            typeof(APrintControlViewModel),
            new FrameworkPropertyMetadata(new PropertyChangedCallback(OnDependencyPropertyChanged)));

        public static readonly DependencyProperty ApproaxNumberOfPagesProperty = DependencyProperty.Register(
            "ApproaxNumberOfPages",
            typeof(int),
            typeof(APrintControlViewModel),
            new FrameworkPropertyMetadata(new PropertyChangedCallback(OnDependencyPropertyChanged)));

        public static readonly DependencyProperty PrintCopyCountProperty = DependencyProperty.Register(
            "PrintCopyCount",
            typeof(int),
            typeof(APrintControlViewModel),
            new FrameworkPropertyMetadata(1, new PropertyChangedCallback(OnDependencyPropertyChanged)));

        public static readonly DependencyProperty PaperSizesProperty = DependencyProperty.Register(
            "PaperSizes",
            typeof(IList<PaperSize>),
            typeof(APrintControlViewModel),
            new FrameworkPropertyMetadata(new PropertyChangedCallback(OnDependencyPropertyChanged)));

        public static readonly DependencyProperty CurrentPaperProperty = DependencyProperty.Register(
            "CurrentPaper",
            typeof(PaperSize),
            typeof(APrintControlViewModel),
            new FrameworkPropertyMetadata(new PropertyChangedCallback(OnDependencyPropertyChanged)));

        public static readonly DependencyProperty DefaultPaperSizeProperty = DependencyProperty.Register(
            "DefaultPaperSize",
            typeof(PaperSize),
            typeof(APrintControlViewModel),
            new FrameworkPropertyMetadata(new PropertyChangedCallback(OnDependencyPropertyChanged)));

        public static readonly DependencyProperty CurrentPrinterNameProperty = DependencyProperty.Register(
            "CurrentPrinterName",
            typeof(string),
            typeof(APrintControlViewModel),
            new FrameworkPropertyMetadata(new PropertyChangedCallback(OnDependencyPropertyChanged)));

        public static readonly DependencyProperty CurrentPrinterProperty = DependencyProperty.Register(
            "CurrentPrinter",
            typeof(PrintQueue),
            typeof(APrintControlViewModel),
            new FrameworkPropertyMetadata(new PropertyChangedCallback(OnDependencyPropertyChanged)));

        public static readonly DependencyProperty PrintersProperty = DependencyProperty.Register(
            "Printers",
            typeof(PrintQueueCollection),
            typeof(APrintControlViewModel),
            new FrameworkPropertyMetadata(new PropertyChangedCallback(OnDependencyPropertyChanged)));

        public static readonly DependencyProperty PagesAcrossProperty = DependencyProperty.Register(
            "PagesAcross",
            typeof(int),
            typeof(APrintControlViewModel),
            new FrameworkPropertyMetadata(new PropertyChangedCallback(OnDependencyPropertyChanged)));

        public static readonly DependencyProperty NumberOfPagesProperty = DependencyProperty.Register(
            "NumberOfPages",
            typeof(int),
            typeof(APrintControlViewModel),
            new FrameworkPropertyMetadata(new PropertyChangedCallback(OnDependencyPropertyChanged)));

        public static readonly DependencyProperty IsPrintingOptionsOpenProperty = DependencyProperty.Register(
            "IsPrintingOptionsOpen",
            typeof(bool),
            typeof(APrintControlViewModel));

        public static readonly DependencyProperty IsCancelPrintingOptionsEnabledProperty = DependencyProperty.Register(
            "IsCancelPrintingOptionsEnabled",
            typeof(bool),
            typeof(APrintControlViewModel));

        public static readonly DependencyProperty IsSetPrintingOptionsEnabledProperty = DependencyProperty.Register(
            "IsSetPrintingOptionsEnabled",
            typeof(bool),
            typeof(APrintControlViewModel));

        public static readonly DependencyProperty CanScaleProperty = DependencyProperty.Register(
            "CanScale",
            typeof(bool),
            typeof(APrintControlViewModel),
            new FrameworkPropertyMetadata(true));

        public static readonly DependencyProperty PrinterErrorVisibilityProperty = DependencyProperty.Register(
            "PrinterErrorVisibility",
            typeof(Visibility),
            typeof(APrintControlViewModel));

        public Visibility PrinterErrorVisibility
        {
            get => (Visibility)this.GetValue(PrinterErrorVisibilityProperty);

            set => this.SetValue(PrinterErrorVisibilityProperty, value);
        }

        public bool CanScale
        {
            get => (bool)this.GetValue(CanScaleProperty);

            set => this.SetValue(CanScaleProperty, value);
        }

        public bool IsPotrait
        {
            get => (bool)this.GetValue(IsPotraitProperty);

            set => this.SetValue(IsPotraitProperty, value);
        }

        public bool IsLandscape
        {
            get => (bool)this.GetValue(IsLandscapeProperty);

            set => this.SetValue(IsLandscapeProperty, value);
        }

        public bool IsAdvancedPrintingOptionOpen
        {
            get => (bool)this.GetValue(IsAdvancedPrintingOptionOpenProperty);

            set => this.SetValue(IsAdvancedPrintingOptionOpenProperty, value);
        }

        public bool IsMarkPageNumbers
        {
            get => (bool)this.GetValue(IsMarkPageNumbersProperty);

            set => this.SetValue(IsMarkPageNumbersProperty, value);
        }

        public bool IsPrintingOptionsOpen
        {
            get => (bool)this.GetValue(IsPrintingOptionsOpenProperty);

            set => this.SetValue(IsPrintingOptionsOpenProperty, value);
        }

        public bool IsCancelPrintingOptionsEnabled
        {
            get => (bool)this.GetValue(IsCancelPrintingOptionsEnabledProperty);

            set => this.SetValue(IsCancelPrintingOptionsEnabledProperty, value);
        }

        public bool IsSetPrintingOptionsEnabled
        {
            get => (bool)this.GetValue(IsSetPrintingOptionsEnabledProperty);

            set => this.SetValue(IsSetPrintingOptionsEnabledProperty, value);
        }

        public string PageOrientationString
        {
            get => (string)this.GetValue(PageOrientationStringProperty);

            set => this.SetValue(PageOrientationStringProperty, value);
        }

        public PageOrientation PageOrientation
        {
            get => (PageOrientation)this.GetValue(PageOrientationProperty);

            set
            {
                this.SetValue(PageOrientationProperty, value);
                this.PageOrientationString = this.PageOrientation.ToString();
            }
        }

        public int ApproaxNumberOfPages
        {
            get => (int)this.GetValue(ApproaxNumberOfPagesProperty);

            set => this.SetValue(ApproaxNumberOfPagesProperty, value);
        }

        public int PrintCopyCount
        {
            get => (int)this.GetValue(PrintCopyCountProperty);

            set => this.SetValue(PrintCopyCountProperty, value);
        }

        public IList<PaperSize> PaperSizes
        {
            get => (IList<PaperSize>)this.GetValue(PaperSizesProperty);

            set => this.SetValue(PaperSizesProperty, value);
        }

        public PaperSize CurrentPaper
        {
            get => (PaperSize)this.GetValue(CurrentPaperProperty);

            set => this.SetValue(CurrentPaperProperty, value);
        }

        public PaperSize DefaultPaperSize
        {
            get => (PaperSize)this.GetValue(DefaultPaperSizeProperty);

            set => this.SetValue(DefaultPaperSizeProperty, value);
        }

        public string CurrentPrinterName
        {
            get => this.GetValue(CurrentPrinterNameProperty).ToString();

            set => this.SetValue(CurrentPrinterNameProperty, value);
        }

        public PrintQueue CurrentPrinter
        {
            get => (PrintQueue)this.GetValue(CurrentPrinterProperty);

            set => this.SetValue(CurrentPrinterProperty, value);
        }

        public PrintQueueCollection Printers
        {
            get => (PrintQueueCollection)this.GetValue(PrintersProperty);

            set => this.SetValue(PrintersProperty, value);
        }

        public int PagesAcross
        {
            get => (int)this.GetValue(PagesAcrossProperty);

            set => this.SetValue(PagesAcrossProperty, value);
        }

        public int NumberOfPages
        {
            get => (int)this.GetValue(NumberOfPagesProperty);

            set => this.SetValue(NumberOfPagesProperty, value);
        }
        #endregion

        #region Member Variables
        public int MaxCopies;
        private bool IsMarkPageNumbersChanged;
        private bool IsPageOrientationChanged;
        private bool IsPrintCopyCountChanged;
        private bool IsCurrentPaperChanged;
        private bool IsCurrentPrinterChanged;
        private bool IsCurrentPrinterNameChanged;

        #endregion

        private static void OnDependencyPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var presenter = (APrintControlViewModel)d;
            if (!presenter.settingOptions)
            {
                switch (e.Property.Name)
                {
                    case "IsMarkPageNumbers":
                        presenter.PrintOptionsSetterIsEnable(true);
                        if (e.OldValue != null && !presenter.IsMarkPageNumbersChanged)
                        {
                            presenter.IsMarkPageNumbersChanged = true;
                            presenter.oldPrintingOptions.IsMarkPageNumbers = (bool)e.OldValue;
                        }

                        presenter._newPrintingOptions.IsMarkPageNumbers = (bool)e.NewValue;
                        break;
                    case "PageOrientation":
                        presenter.PrintOptionsSetterIsEnable(true);
                        if (e.OldValue != null && !presenter.IsPageOrientationChanged)
                        {
                            presenter.IsPageOrientationChanged = true;
                            presenter.oldPrintingOptions.PageOrientation = (PageOrientation)e.OldValue;
                        }

                        presenter._newPrintingOptions.PageOrientation = (PageOrientation)e.NewValue;
                        break;
                    case "CurrentPaper":
                        presenter.PrintOptionsSetterIsEnable(true);
                        if (e.OldValue != null && !presenter.IsCurrentPaperChanged)
                        {
                            presenter.IsCurrentPaperChanged = true;
                            presenter.oldPrintingOptions.CurrentPaper = (PaperSize)e.OldValue;
                        }

                        presenter._newPrintingOptions.CurrentPaper = (PaperSize)e.NewValue;
                        break;
                    case "PrintCopyCount":
                        presenter.PrintOptionsSetterIsEnable(true);
                        if (e.OldValue != null && !presenter.IsPrintCopyCountChanged)
                        {
                            presenter.IsPrintCopyCountChanged = true;
                            presenter.oldPrintingOptions.PrintCopyCount = (int)e.OldValue;
                        }

                        presenter._newPrintingOptions.PrintCopyCount = (int)e.NewValue;
                        break;
                    case "CurrentPrinter":
                        if (e.NewValue != null)
                        {
                            presenter.PrintOptionsSetterIsEnable(true);
                            try
                            {
                                if (e.OldValue != null && !presenter.IsCurrentPrinterChanged)
                                {
                                    presenter.IsCurrentPrinterChanged = true;
                                    presenter.oldPrintingOptions.CurrentPrinter = (PrintQueue)e.OldValue;
                                }

                                presenter.FetchSetting();
                                presenter._newPrintingOptions.CurrentPrinter = (PrintQueue)e.NewValue;
                                presenter.SetPrintError(false);
                            }
                            catch (Exception)
                            {
                                presenter.SetPrintError(true);
                            }
                        }

                        break;
                    case "CurrentPrinterName":
                        presenter.PrintOptionsSetterIsEnable(true);
                        if (e.OldValue != null && !presenter.IsCurrentPrinterNameChanged)
                        {
                            presenter.IsCurrentPrinterNameChanged = true;
                            presenter.oldPrintingOptions.CurrentPrinterName = (string)e.OldValue;
                        }

                        presenter._newPrintingOptions.CurrentPrinterName = (string)e.NewValue;
                        break;
                }
            }
            else
            {
                if (e.Property.Name == "CurrentPrinter")
                {
                    var currentPrinter = e.NewValue as PrintQueue;
                    if (currentPrinter != null)
                    {
                        try
                        {
                            presenter.FetchSetting();
                            presenter.SetPrintError(false);
                        }
                        catch (Exception)
                        {
                            presenter.SetPrintError(true);
                        }
                    }
                }
            }
        }

        private void SetPrintError(bool isError)
        {
            if (isError)
            {
                this.PrinterErrorVisibility = Visibility.Visible;
                ((PrintControlView)this.View).EnablePrintingOptionsSet(false);
                ((PrintControlView)this.View).PrintingOptionsWaitCurtainVisibility(false);
            }
            else
            {
                this.PrinterErrorVisibility = Visibility.Hidden;
                ((PrintControlView)this.View).EnablePrintingOptionsSet(true);
            }
        }

        public virtual void HandlePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }
        #region Class Members

        public PrintQueue LocalPrinter;
        public Window FullScreenPrintWindow;
        private PrintingOptions oldPrintingOptions;
        private PrintingOptions _newPrintingOptions;
        private bool settingOptions;
        public bool Loading;
        public bool ReloadingPreview;

        public ICommand PrintDocumentCommand { get; set; }

        public ICommand PrintSetupCommand { get; set; }

        public ICommand CancelPrintCommand { get; set; }

        public ICommand PageOrientationCommand { get; set; }

        public ICommand SetPrintingOptionsCommand { get; set; }

        public ICommand CancelPrintingOptionsCommand { get; set; }

        public ICommand MarkPageNumbersCommand { get; set; }

        public ICommand AllPagesCommand { get; set; }

        public ICommand ActualPageSizeCommand { get; set; }
        #endregion

        protected APrintControlViewModel(PrintControlView view)
            : base(view)
        {
            this.PrintControlView = view;
            this.PrintControlView.Loaded += this.PrintControlViewLoaded;
            this.oldPrintingOptions = new PrintingOptions();
            this._newPrintingOptions = new PrintingOptions();

            this.WaitScreen = new WaitScreenViewModel(new WaitScreenView());
            this.PrintUtility = new PrintUtility();
            this.CancelPrintCommand = new DelegateCommand<object>(this.ExecuteCancelPrint);
            this.PrintDocumentCommand = new DelegateCommand<object>(this.ExecutePrint);
            this.PageOrientationCommand = new DelegateCommand<object>(this.ExecutePageOrientation);
            this.SetPrintingOptionsCommand = new DelegateCommand<object>(this.ExecuteSetPrintingOptions);
            this.CancelPrintingOptionsCommand = new DelegateCommand<object>(this.ExecuteCancelPrintingOptions);
            this.MarkPageNumbersCommand = new DelegateCommand<object>(this.ExecuteMarkPageNumbers);
            this.AllPagesCommand = new DelegateCommand<object>(this.ExecuteAllPages);
            this.ActualPageSizeCommand = new DelegateCommand<object>(this.ExecuteActualPageSizeCommand);
            this.ChangePaperCommand = new DelegateCommand<object>(this.ExecuteChangePaper);
        }

        private void ExecuteChangePaper(object obj)
        {
            try
            {
                var ptc = new PrintTicketConverter(this.CurrentPrinter.FullName, this.CurrentPrinter.ClientPrintSchemaVersion);
                var mainWindowPtr = new WindowInteropHelper(this.FullScreenPrintWindow).Handle;
                var myDevMode = ptc.ConvertPrintTicketToDevMode(this.CurrentPrinter.UserPrintTicket, BaseDevModeType.UserDefault);
                var pinnedDevMode = GCHandle.Alloc(myDevMode, GCHandleType.Pinned);
                var pDevMode = pinnedDevMode.AddrOfPinnedObject();
                var result = DocumentProperties(mainWindowPtr, IntPtr.Zero, this.CurrentPrinter.FullName, pDevMode, pDevMode, 14);
                if (result == 1)
                {
                    this.CurrentPrinter.UserPrintTicket = ptc.ConvertDevModeToPrintTicket(myDevMode);
                    pinnedDevMode.Free();
                    this.PrintCopyCount = this.CurrentPrinter.UserPrintTicket.CopyCount.Value;
                    this.SetPageOrientation(this.CurrentPrinter.UserPrintTicket.PageOrientation);
                    this.SetCurrentPaper(this.CurrentPrinter.UserPrintTicket.PageMediaSize);
                    this.ExecuteSetPrintingOptions(null);
                }
            }
            catch (Exception)
            {
            }
        }

        public void ExecutePageOrientation(object parameter)
        {
            this.PageOrientation = parameter.ToString() == "Portrait" ? PageOrientation.Portrait : PageOrientation.Landscape;
        }

        protected virtual void ExecuteActualPageSizeCommand(object obj)
        {
            this.ShowAllPages = false;
            this.ReloadPreview();
        }

        public abstract void ReloadPreview();

        public virtual void InitializeProperties()
        {
            try
            {
                this.Printers = this.PrintUtility.GetPrinters();
                this.SetLocalPrinter();
                var defaultPrintQueue = this.PrintUtility.GetDefaultPrintQueue(string.Empty);
                this.IsMarkPageNumbers = true;
                var defaultPrinterFullName = defaultPrintQueue.FullName;
                var defaultExists = false;
                foreach (var printer in this.Printers)
                {
                    if (printer.Name == defaultPrintQueue.Name)
                    {
                        defaultExists = true;
                        break;
                    }
                }

                if (!defaultExists)
                {
                    this.Printers.Add(defaultPrintQueue);
                    var temp = this.Printers;
                    this.Printers = null;
                    this.Printers = temp;
                }

                this.CurrentPrinterName = defaultPrinterFullName;
                this.CurrentPrinter = this.Printers.First(e => e.FullName == defaultPrinterFullName);
                this.PrintOptionsSetterIsEnable(false);
                this.SetPrintError(false);
                var userPrintTicket = this.PrintUtility.GetUserPrintTicket(this.CurrentPrinter.FullName);
                if (userPrintTicket != null)
                {
                    this.CurrentPrinter.UserPrintTicket = userPrintTicket;
                }

                this.SetCurrentPaper(this.CurrentPrinter.UserPrintTicket.PageMediaSize);
                this.SetPageOrientation(this.CurrentPrinter.UserPrintTicket.PageOrientation);
                this.ExecuteSetPrintingOptions(false);
            }
            catch
            {
                this.SetPrintError(true);
            }
        }

        private void SetLocalPrinter()
        {
            foreach (var printer in this.Printers)
            {
                if (printer.HostingPrintServer.Name.Contains(SystemInformation.ComputerName))
                {
                    this.LocalPrinter = printer;
                    break;
                }
            }
        }

        #region Command Execution
        public virtual void ExecuteAllPages(object parameter)
        {
            this.ShowAllPages = true;
            this.ReloadPreview();
        }

        private void SetPageOrientation(PageOrientation? pageOrientation)
        {
            if (pageOrientation == PageOrientation.Portrait && this.PageOrientation != PageOrientation.Portrait)
            {
                this.PageOrientation = PageOrientation.Portrait;
            }

            if (pageOrientation == PageOrientation.Landscape && this.PageOrientation != PageOrientation.Landscape)
            {
                this.PageOrientation = PageOrientation.Landscape;
            }
        }

        private void SetCurrentPaper(PageMediaSize pageMediaSize)
        {
            var widthInInch = Math.Round(pageMediaSize.Width.Value / 96 * 100);
            var heightInInch = Math.Round(pageMediaSize.Height.Value / 96 * 100);
            var paperSize = this.PaperSizes.FirstOrDefault(p => p.Width == widthInInch && p.Height == heightInInch);
            if (paperSize != null)
            {
                this.CurrentPaper = this.PaperSizes[this.PaperSizes.IndexOf(paperSize)];
            }
        }

        public void ExecuteSetPrintingOptions(object parameter)
        {
            this.settingOptions = true;
            if (this.IsPrintCopyCountChanged)
            {
                this.PrintCopyCount = this._newPrintingOptions.PrintCopyCount;
                this.CurrentPrinter.UserPrintTicket.CopyCount = this.PrintCopyCount;
            }

            if (this.IsMarkPageNumbersChanged)
            {
                this.IsMarkPageNumbers = this._newPrintingOptions.IsMarkPageNumbers;
            }

            if (this.IsPageOrientationChanged)
            {
                this.PageOrientation = this._newPrintingOptions.PageOrientation;
                if (this.PageOrientation == PageOrientation.Portrait)
                {
                    ((PrintControlView)this.View).Portrait.IsChecked = true;
                }
                else
                {
                    ((PrintControlView)this.View).Landscape.IsChecked = true;
                }

                this.SetupPrintOrientation(this.PageOrientation);
            }

            if (this.IsCurrentPaperChanged)
            {
                this.CurrentPaper = this._newPrintingOptions.CurrentPaper;
            }

            this.PrintUtility.SaveUserPrintTicket(this.CurrentPrinter);

            this.ResetPrintingOptions();
            this.ReloadPreview();
        }

        public void ExecuteCancelPrintingOptions(object parameter)
        {
            this.settingOptions = true;
            if (this.IsPrintCopyCountChanged)
            {
                this.PrintCopyCount = this.oldPrintingOptions.PrintCopyCount;
            }

            if (this.IsMarkPageNumbersChanged)
            {
                this.IsMarkPageNumbers = this.oldPrintingOptions.IsMarkPageNumbers;
            }

            if (this.IsPageOrientationChanged)
            {
                this.PageOrientation = this.oldPrintingOptions.PageOrientation;
                if (this.PageOrientation == PageOrientation.Portrait)
                {
                    ((PrintControlView)this.View).Portrait.IsChecked = true;
                }
                else
                {
                    ((PrintControlView)this.View).Landscape.IsChecked = true;
                }
            }

            if (this.IsCurrentPrinterChanged)
            {
                this.PaperSizes = this.oldPrintingOptions.PaperSizes;
                this.CurrentPrinter = this.oldPrintingOptions.CurrentPrinter;
                this.CurrentPrinterName = this.oldPrintingOptions.CurrentPrinterName;
                this.CurrentPaper = this.oldPrintingOptions.CurrentPaper;
            }

            this.ResetPrintingOptions();
            this.settingOptions = false;
        }

        public void ResetPrintingOptions()
        {
            this.settingOptions = true;
            this.IsSetPrintingOptionsEnabled = false;
            this.IsCancelPrintingOptionsEnabled = false;
            this.IsAdvancedPrintingOptionOpen = false;
            this.oldPrintingOptions = new PrintingOptions();
            this._newPrintingOptions = new PrintingOptions();
            this.IsMarkPageNumbersChanged = false;
            this.IsPageOrientationChanged = false;
            this.IsPrintCopyCountChanged = false;
            this.IsCurrentPaperChanged = false;
            this.IsCurrentPrinterChanged = false;
            this.IsCurrentPrinterNameChanged = false;
            this.settingOptions = false;
            this.PrintOptionsSetterIsEnable(false);
            ((PrintControlView)this.View).EnablePrintingOptionsSet(true);
        }

        private void PrintOptionsSetterIsEnable(bool isEnabled)
        {
            ((PrintControlView)this.View).SetButton.Visibility = Visibility.Visible;
            ((PrintControlView)this.View).SetButton.IsEnabled = isEnabled;
            ((PrintControlView)this.View).CancelSetButton.IsEnabled = isEnabled;
        }

        public virtual void ExecuteMarkPageNumbers(object parameter)
        {
            this.ReloadPreview();
        }

        public void ExecuteCancelPrint(object parameter)
        {
            if (this.FullScreenPrintWindow != null)
            {
                this.FullScreenPrintWindow.Close();
            }
        }

        #region Exec Print

        public virtual void ExecutePrint(object parameter)
        {
            try
            {
                var printDialog = new System.Windows.Controls.PrintDialog();
                printDialog.PrintQueue = this.CurrentPrinter;
                printDialog.PrintTicket = this.CurrentPrinter.UserPrintTicket;
                this.ShowProgressDialog();
                printDialog.PrintDocument(this.Paginator, string.Empty);
            }
            catch (Exception)
            {
            }
            finally
            {
                this.ProgressDialog.Hide();
            }
        }

        private void SetupPrintOrientation(PageOrientation orientation)
        {
            if (orientation == PageOrientation.Portrait)
            {
                this.CurrentPrinter.UserPrintTicket.PageOrientation = PageOrientation.Portrait;
            }
            else
            {
                this.CurrentPrinter.UserPrintTicket.PageOrientation = PageOrientation.Landscape;
            }
        }

        public void ShowProgressDialog()
        {
            this.ProgressDialog = new ProgressDialogViewModel(new ProgressDialogView());
            var cancelAsyncPrintCommand = new DelegateCommand<object>(this.ExecuteCancelAsyncPrint);
            this.ProgressDialog.CancelCommand = cancelAsyncPrintCommand;
            this.ProgressDialog.MaxProgressValue = this.ApproaxNumberOfPages;
            this.ProgressDialog.CurrentProgressValue = 0;
            this.ProgressDialog.Message = this.GetStatusMessage();
            this.ProgressDialog.DialogTitle = TMP.PrintEngine.Resources.Strings.Printing;
            this.ProgressDialog.CancelButtonCaption = TMP.PrintEngine.Resources.Strings.CancelButtonCaption;
            this.SetProgressDialogCancelButtonVisibility();
            this.ProgressDialog.Show();
        }

        public virtual void SetProgressDialogCancelButtonVisibility()
        {
            this.ProgressDialog.CancelButtonVisibility = this.CurrentPrinter.IsXpsDevice ? Visibility.Visible : Visibility.Hidden;
        }

        public virtual void ExecuteCancelAsyncPrint(object obj)
        {
            try
            {
                this.ProgressDialog.Hide();
            }
            catch
            {
            }
        }

        public string GetStatusMessage()
        {
            return string.Format("{0} {1} / {2}", TMP.PrintEngine.Resources.Strings.PrintingPages,
                this.ProgressDialog.CurrentProgressValue, this.ProgressDialog.MaxProgressValue);
        }

        #endregion

        #endregion

        #region Show View

        public void CreatePrintPreviewWindow()
        {
            this.FullScreenPrintWindow = new Window();
            this.FullScreenPrintWindow.Activated += this.FullScreenPrintWindowActivated;
            this.FullScreenPrintWindow.Closing += this.FullScreenPrintWindowClosing;
            this.FullScreenPrintWindow.Title = TMP.PrintEngine.Resources.Strings.PrintPreview;
            this.FullScreenPrintWindow.MinWidth = 600;
            this.FullScreenPrintWindow.MinHeight = 600;
            this.FullScreenPrintWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            this.FullScreenPrintWindow.ShowInTaskbar = false;
            this.FullScreenPrintWindow.WindowStyle = WindowStyle.SingleBorderWindow;
            this.FullScreenPrintWindow.WindowState = WindowState.Maximized;
            this.FullScreenPrintWindow.Owner = Application.Current.MainWindow;
            this.FullScreenPrintWindow.Content = this.View;
            ApplicationExtention.MainWindow = this.FullScreenPrintWindow;
        }

        public virtual void FullScreenPrintWindowClosing(object sender, CancelEventArgs e)
        {
            if (!this.ReloadingPreview)
            {
                this.IsAdvancedPrintingOptionOpen = false;
            }
        }

        private void FullScreenPrintWindowActivated(object sender, EventArgs e)
        {
            if (this.Loading)
            {
                ApplicationExtention.MainWindow = this.FullScreenPrintWindow;
            }
        }

        private void LoadDocument()
        {
            ////TempFileLogger.Log("Starting Load Document");
            this.settingOptions = true;
            this.ReloadPreview();
            this.Loading = false;
            this.IsSetPrintingOptionsEnabled = false;
            this.IsCancelPrintingOptionsEnabled = false;
            this.settingOptions = false;
            ////TempFileLogger.Log("End Load Document");
        }
        #endregion

        #region updating pane visibility
        private Dispatcher dispatcher;
        private const bool IsShown = false;
        private System.Timers.Timer hideTimer;

        public void ShowPrintOptionCurtain()
        {
            if (this.dispatcher == null)
            {
                this.dispatcher = Application.Current.Dispatcher;
                this.hideTimer = new System.Timers.Timer();
                this.hideTimer.Elapsed += this.HideTimerElapsed;
                this.hideTimer.Interval = 300;
                this.hideTimer.Enabled = false;
            }

            if (this.dispatcher != null)
            {
                this.dispatcher.BeginInvoke(DispatcherPriority.Send, new Action(this.ShowUpdatingPaneHandler));
                Block();
            }
        }

        private static void Block()
        {
            if (Application.Current == null)
            {
                return;
            }

            while (true)
            {
                ApplicationExtention.DoEvents(Application.Current);
                System.Threading.Thread.Sleep(5);
                break;
            }
        }

        private void ShowUpdatingPaneHandler()
        {
            try
            {
                if (IsShown)
                {
                    this.hideTimer.Stop();
                    return;
                }

                ((IPrintControlView)this.View).PrintingOptionsWaitCurtainVisibility(true);
            }
            catch
            {
            }
        }

        public void HidePrintOptionCurtain()
        {
            this.hideTimer.Start();
        }

        private void HideTimerElapsed(object sender, ElapsedEventArgs e)
        {
            this.hideTimer.Stop();
            if (this.dispatcher != null)
            {
                this.dispatcher.BeginInvoke(DispatcherPriority.Send, new Action(this.HideUpdatingPaneHandler));
            }
        }

        private void HideUpdatingPaneHandler()
        {
            ((IPrintControlView)this.View).PrintingOptionsWaitCurtainVisibility(false);
        }
        #endregion

        #region Asynchronous Printer Data

        public IDocumentPaginatorSource PaginatorSource;
        public double Pageaccrosswith;
        protected PrintControlView PrintControlView;
        protected bool ShowAllPages = true;
        private readonly PrinterPreferences printerPreferences;
        protected PrintUtility PrintUtility;

        public void FetchSetting()
        {
            this.ShowPrintOptionCurtain();
            this.CurrentPrinterName = this.CurrentPrinter.FullName;
            this.PaperSizes = this.PrintUtility.GetPaperSizes(this.CurrentPrinterName);
            var userPrintTicket = this.PrintUtility.GetUserPrintTicket(this.CurrentPrinter.FullName);
            if (userPrintTicket != null)
            {
                this.CurrentPrinter.UserPrintTicket = userPrintTicket;
            }

            this.SetCurrentPaper(this.CurrentPrinter.UserPrintTicket.PageMediaSize);
            this.SetPageOrientation(this.CurrentPrinter.UserPrintTicket.PageOrientation);
            this.PrintCopyCount = this.CurrentPrinter.UserPrintTicket.CopyCount != null ? this.CurrentPrinter.UserPrintTicket.CopyCount.Value : this.PrintCopyCount;
            this.ExecuteSetPrintingOptions(null);
            this.HidePrintOptionCurtain();
        }

        #endregion

        protected void PrintControlViewLoaded(object sender, RoutedEventArgs e)
        {
            this.PrintControlView.SetPrintingOptionsWaitCurtainVisibility(Visibility.Collapsed);
            this.InitializeProperties();
            this.ResetPrintingOptions();
            this.LoadDocument();
        }

        protected void DisplayPagePreviewsAll(DocumentPaginator paginator)
        {
            double scale;
            var rowCount = this.GetRowCount(paginator);
            var container = ((IPrintControlView)this.View).GetPagePreviewContainer();
            container.Children.Clear();
            for (var i = 0; i < rowCount; i++)
            {

                container.Children.Add(new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = System.Windows.VerticalAlignment.Center,
                });
                Application.Current.DoEvents();
            }

            var totalWidth = this.PagesAcross * (paginator.PageSize.Width + 40);
            var totalHeight = rowCount * (paginator.PageSize.Height + 40);
            if (totalWidth > totalHeight)
            {
                scale = ((ScrollViewer)((Border)container.Parent).Parent).ActualWidth / totalWidth;
            }
            else
            {
                scale = ((ScrollViewer)((Border)container.Parent).Parent).ActualHeight / totalHeight;
            }

            scale = this.ShowAllPages ? scale : 1;

            for (var i = 0; i < paginator.PageCount; i++)
            {
                Application.Current.DoEvents();
                var pageElement = this.GetPageUiElement(i, paginator, scale);
                pageElement.HorizontalAlignment = HorizontalAlignment.Center;
                pageElement.VerticalAlignment = System.Windows.VerticalAlignment.Center;
                var rowIndex = i / this.PagesAcross;
                InsertPageToPreviewContainer(rowIndex, pageElement, container);
            }
        }

        private static void InsertPageToPreviewContainer(int rowIndex, Border pageElement, StackPanel container)
        {
            ((StackPanel)container.Children[rowIndex]).Children.Add(pageElement);
        }

        private int GetRowCount(DocumentPaginator paginator)
        {
            return (int)Math.Ceiling((double)paginator.PageCount / this.PagesAcross);
        }

        private Border GetPageUiElement(int i, DocumentPaginator paginator, double scale)
        {
            var source = paginator.GetPage(i);
            var border = new Border() { Background = Brushes.White };
            border.Margin = new Thickness(10 * scale);
            border.BorderBrush = Brushes.DarkGray;
            border.BorderThickness = new Thickness(1);

            // var margin = PrintUtility.GetPageMargin(CurrentPrinterName);
            var margin = new Thickness();
            var rectangle = new Rectangle();
            rectangle.Width = ((source.Size.Width * 0.96) - (margin.Left + margin.Right)) * scale;
            rectangle.Height = ((source.Size.Height * 0.96) - (margin.Top + margin.Bottom)) * scale;
            rectangle.Margin = new Thickness(margin.Left * scale, margin.Top * scale, margin.Right * scale, margin.Bottom * scale);
            rectangle.Fill = Brushes.White;
            var vb = new VisualBrush(source.Visual);
            vb.Opacity = 1;
            vb.Stretch = Stretch.Uniform;
            rectangle.Fill = vb;
            border.Child = rectangle;
            return border;
        }

        public void PrintControlLoaded()
        {
            this.LoadDocument();
        }
    }
}