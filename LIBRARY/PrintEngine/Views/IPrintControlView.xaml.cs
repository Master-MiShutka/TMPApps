namespace TMP.PrintEngine.Views
{
    using System.Windows.Controls;
    using System.Windows.Media;

    public interface IPrintControlView : IView
    {
        DocumentViewer DocumentViewer { get; }

        void PrintingOptionsWaitCurtainVisibility(bool b);

        StackPanel GetPagePreviewContainer();

        void ScalePreviewNode(ScaleTransform scaleTransform);
    }
}