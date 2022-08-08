namespace WpfMouseWheel.Windows
{
    using System.Windows;
    using System.Windows.Controls;

    /// <summary>
    /// Represents a common interface for FlowDocumentScrollViewer and FlowDocumentPageViewer classes
    /// </summary>
    public interface IZoomElement : IFrameworkLevelElement
    {
        bool CanIncreaseZoom
        {
            get;
        }

        bool CanDecreaseZoom
        {
            get;
        }

        double MinZoom
        {
            get; set;
        }

        double MaxZoom
        {
            get; set;
        }

        double Zoom
        {
            get; set;
        }

        double ZoomIncrement
        {
            get; set;
        }

        void IncreaseZoom();

        void DecreaseZoom();
    }

    /// <summary>
    /// Acts as a factory for FlowDocumentScrollViewerProxy and FlowDocumentPageViewerProxy
    /// </summary>
    public abstract class ZoomElementFactory : FrameworkLevelElementFactory
    {
        public static new IZoomElement FromElement(DependencyObject element)
        {
            if (element is FlowDocumentScrollViewer)
            {
                return new FlowDocumentScrollViewerProxy(element as FlowDocumentScrollViewer);
            }
            else if (element is FlowDocumentPageViewer)
            {
                return new FlowDocumentPageViewerProxy(element as FlowDocumentPageViewer);
            }
            else
            {
                return null;
            }
        }

        protected ZoomElementFactory(DependencyObject proxied)
            : base(proxied)
        {
        }

        protected class FlowDocumentScrollViewerProxy : FrameworkElementProxy, IZoomElement
        {
            public FlowDocumentScrollViewerProxy(FlowDocumentScrollViewer proxied)
                : base(proxied) { }

            public bool CanIncreaseZoom => this.Handle.CanIncreaseZoom;

            public bool CanDecreaseZoom => this.Handle.CanDecreaseZoom;

            public double MinZoom
            {
                get => this.Handle.MinZoom;
                set => this.Handle.MinZoom = value;
            }

            public double MaxZoom
            {
                get => this.Handle.MaxZoom;
                set => this.Handle.MaxZoom = value;
            }

            public double Zoom
            {
                get => this.Handle.Zoom;
                set => this.Handle.Zoom = value;
            }

            public double ZoomIncrement
            {
                get => this.Handle.ZoomIncrement;
                set => this.Handle.ZoomIncrement = value;
            }

            public void IncreaseZoom()
            {
                this.Handle.IncreaseZoom();
            }

            public void DecreaseZoom()
            {
                this.Handle.DecreaseZoom();
            }

            private FlowDocumentScrollViewer Handle => this.Proxied as FlowDocumentScrollViewer;
        }

        protected class FlowDocumentPageViewerProxy : FrameworkElementProxy, IZoomElement
        {
            public FlowDocumentPageViewerProxy(FlowDocumentPageViewer proxied)
                : base(proxied) { }

            public bool CanIncreaseZoom => this.Handle.CanIncreaseZoom;

            public bool CanDecreaseZoom => this.Handle.CanDecreaseZoom;

            public double MinZoom
            {
                get => this.Handle.MinZoom;
                set => this.Handle.MinZoom = value;
            }

            public double MaxZoom
            {
                get => this.Handle.MaxZoom;
                set => this.Handle.MaxZoom = value;
            }

            public double Zoom
            {
                get => this.Handle.Zoom;
                set => this.Handle.Zoom = value;
            }

            public double ZoomIncrement
            {
                get => this.Handle.ZoomIncrement;
                set => this.Handle.ZoomIncrement = value;
            }

            public void IncreaseZoom()
            {
                this.Handle.IncreaseZoom();
            }

            public void DecreaseZoom()
            {
                this.Handle.DecreaseZoom();
            }

            private FlowDocumentPageViewer Handle => this.Proxied as FlowDocumentPageViewer;
        }
    }
}
