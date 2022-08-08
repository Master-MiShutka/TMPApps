namespace TMP.PrintEngine.ViewModels
{
    using System;
    using System.Data;
    using System.Drawing.Printing;
    using System.Printing;
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Media;
    using TMP.PrintEngine.Extensions;
    using TMP.PrintEngine.Paginators;
    using TMP.PrintEngine.Utils;
    using TMP.PrintEngine.Views;

    public class PrintControlViewModel : APrintControlViewModel, IPrintControlViewModel
    {
        #region Commands

        public DrawingVisual DrawingVisual { get; set; }

        public ICommand ResizeCommand { get; set; }

        public ICommand ApplyScaleCommand { get; set; }

        public ICommand CancelScaleCommand { get; set; }
        #endregion

        #region Dependency Properties
        public double Scale
        {
            get => (double)this.GetValue(ScaleProperty);

            set => this.SetValue(ScaleProperty, value);
        }

        public static readonly DependencyProperty ScaleProperty = DependencyProperty.Register(
            "Scale",
            typeof(double),
            typeof(PrintControlViewModel),
            new FrameworkPropertyMetadata(new PropertyChangedCallback(OnPropertyChanged)));

        private bool isCancelPrint;

        #endregion

        public double ScaleFactor { get; set; }

        private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((PrintControlViewModel)d).HandlePropertyChanged(d, e);
        }

        public PrintControlViewModel(PrintControlView view)
            : base(view)
        {

            this.ResizeCommand = new DelegateCommand<object>(this.ExecuteResize);
            this.ApplyScaleCommand = new DelegateCommand<object>(this.ExecuteApplyScale);
            this.CancelScaleCommand = new DelegateCommand<object>(this.ExecuteCancelScale);
            this.PrintControlView.ResizeButtonVisibility(true);
            this.PrintControlView.SetPageNumberVisibility(Visibility.Visible);
        }

        public void ExecuteResize(object parameter)
        {
            this.PrintControlView.ScalePreviewPaneVisibility(true);
        }

        private void ExecuteCancelScale(object parameter)
        {
            this.ScaleCanceling = true;
            this.Scale = this.OldScale;
            this.PrintControlView.ScalePreviewPaneVisibility(false);
            this.ScaleCanceling = false;
        }

        private void ExecuteApplyScale(object parameter)
        {
            this.OldScale = this.Scale;
            this.PrintControlView.ScalePreviewPaneVisibility(false);
            this.ReloadPreview();
        }

        public override void InitializeProperties()
        {
            this.ResetScale();
            base.InitializeProperties();
        }

        private void ResetScale()
        {
            this.OldScale = 1;
            this.Scale = 1;
            this.PrintControlView.ScalePreviewPaneVisibility(false);
        }

        public override void ReloadPreview()
        {
            if (this.CurrentPaper != null)
            {
                this.ReloadPreview(this.Scale, new Thickness(), this.PageOrientation, this.CurrentPaper);
            }
        }

        public void ReloadPreview(double scale, Thickness margin, PageOrientation pageOrientation, PaperSize paperSize)
        {
            try
            {
                this.ReloadingPreview = true;
                this.ShowWaitScreen();
                var printSize = GetPrintSize(paperSize, pageOrientation);
                var visual = this.GetScaledVisual(scale);
                this.CreatePaginator(visual, printSize, margin);
                var visualPaginator = (VisualPaginator)this.Paginator;
                visualPaginator.Initialize(this.IsMarkPageNumbers);
                this.PagesAcross = visualPaginator.HorizontalPageCount;

                this.ApproaxNumberOfPages = this.MaxCopies = this.Paginator.PageCount;
                if (this.Scale == 1)
                {
                    this.NumberOfPages = this.ApproaxNumberOfPages;
                }

                this.DisplayPagePreviewsAll(visualPaginator);
                this.ReloadingPreview = false;
            }
            catch
            {
            }
            finally
            {
                this.WaitScreen.Hide();
            }
        }

        private DrawingVisual GetScaledVisual(double scale)
        {
            if (scale == 1)
            {
                return this.DrawingVisual;
            }

            var visual = new DrawingVisual();
            using (var dc = visual.RenderOpen())
            {
                dc.PushTransform(new ScaleTransform(scale, scale));
                dc.DrawDrawing(this.DrawingVisual.Drawing);
            }

            return visual;
        }

        private static Size GetPrintSize(PaperSize paperSize, PageOrientation pageOrientation)
        {
            var printSize = new Size(paperSize.Width, paperSize.Height);
            if (pageOrientation == PageOrientation.Landscape)
            {
                printSize = new Size(paperSize.Height, paperSize.Width);
            }

            return printSize;
        }

        private void ShowWaitScreen()
        {
            if (this.FullScreenPrintWindow != null)
            {
                this.WaitScreen.Show(TMP.PrintEngine.Resources.Strings.WaitMessage);
            }
        }

        protected virtual void CreatePaginator(DrawingVisual visual, Size printSize, Thickness margin)
        {
            this.Paginator = new VisualPaginator(visual, printSize, margin, this.PrintUtility.GetPageMargin(this.CurrentPrinterName));
        }

        public override void HandlePropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            var presenter = (PrintControlViewModel)o;
            switch (e.Property.Name)
            {
                case "Scale":
                    if (presenter.ScaleCanceling)
                    {
                        return;
                    }

                    ((IPrintControlView)presenter.View).ScalePreviewNode(new ScaleTransform(presenter.Scale, presenter.Scale));
                    presenter.ApproaxNumberOfPages = Convert.ToInt32(Math.Ceiling(presenter.NumberOfPages * presenter.Scale));
                    break;
            }

            base.HandlePropertyChanged(o, e);
        }

        public override void ExecutePrint(object parameter)
        {
            try
            {
                var printDialog = new System.Windows.Controls.PrintDialog();
                printDialog.PrintQueue = this.CurrentPrinter;
                printDialog.PrintTicket = this.CurrentPrinter.UserPrintTicket;
                this.ShowProgressDialog();
                ((VisualPaginator)this.Paginator).PageCreated += this.PrintControlPresenterPageCreated;
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

        private void PrintControlPresenterPageCreated(object sender, PageEventArgs e)
        {
            this.ProgressDialog.CurrentProgressValue = e.PageNumber;
            this.ProgressDialog.Message = this.GetStatusMessage();
            Application.Current.DoEvents();
        }

        public override void SetProgressDialogCancelButtonVisibility()
        {
            this.ProgressDialog.CancelButtonVisibility = Visibility.Visible;
        }

        public void ShowPrintPreview()
        {
            if (this.FullScreenPrintWindow != null)
            {
                this.FullScreenPrintWindow.Content = null;
            }

            this.CreatePrintPreviewWindow();
            this.Loading = true;
            this.IsSetPrintingOptionsEnabled = false;
            this.IsCancelPrintingOptionsEnabled = false;
            if (this.FullScreenPrintWindow != null)
            {
                this.FullScreenPrintWindow.ShowDialog();
            }

            ApplicationExtention.MainWindow = null;
        }

        public void ShowPrintPreview(DataTable dataTable)
        {
            DataTableUtil.Validate(dataTable);
        }
    }
}