namespace TMP.PrintEngine.Views
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using TMP.PrintEngine.ViewModels;

    /// <summary>
    /// Interaction logic for AdvancedColorPicker.xaml
    /// </summary>
    public partial class PrintControlView : IPrintControlView
    {
        public PrintControlView()
        {
            this.InitializeComponent();
        }

        #region IView Members

        private APrintControlViewModel viewModel;

        public IViewModel ViewModel
        {
            get => this.viewModel;

            set
            {
                this.viewModel = value as APrintControlViewModel;
                this.DataContext = this.viewModel;
            }
        }

        public void SetPageNumberVisibility(Visibility visibility)
        {
            this.PageNumberMarker.Visibility = visibility;
        }

        public void SetPrintingOptionsWaitCurtainVisibility(Visibility visibility)
        {
            this.PrintingOptionsWaitCurtain.Visibility = visibility;
        }

        #endregion

        #region IPrintControlView Members

        DocumentViewer IPrintControlView.DocumentViewer => null; // DocViewer;

        public StackPanel GetPagePreviewContainer()
        {
            return this.PagePreviewContainer;
        }

        public ScrollViewer GetSv()
        {
            return null; // sv;
        }

        #endregion

        public void ScalePreviewPaneVisibility(bool isVisible)
        {
        }

        public void ResizeButtonVisibility(bool isVisible)
        {
        }

        public void PrintingOptionsWaitCurtainVisibility(bool isVisible)
        {
            if (isVisible)
            {
                this.PrintingOptionsWaitCurtain.Visibility = Visibility.Visible;
            }
            else
            {
                this.PrintingOptionsWaitCurtain.Visibility = Visibility.Collapsed;
            }
        }

        public void ScalePreviewNode(ScaleTransform scaleTransform)
        {
            this.PreviewNode.LayoutTransform = scaleTransform;
        }

        internal void EnablePrintingOptionsSet(bool isEnabled)
        {
            if (isEnabled)
            {
                this.SetPanel.Visibility = Visibility.Visible;
            }
            else
            {
                this.SetPanel.Visibility = Visibility.Collapsed;
            }
        }
    }
}