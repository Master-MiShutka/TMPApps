namespace WpfMouseWheel.Windows
{
    using System;
    using System.Windows;

    /// <summary>
    /// Represents a common interface for FrameworkElement and FrameworkContentElement classes
    /// </summary>
    public interface IFrameworkLevelElement : IInputLevelElement
    {
        event EventHandler Initialized;

        event RoutedEventHandler Loaded;

        event RoutedEventHandler Unloaded;

        bool IsInitialized
        {
            get;
        }

        bool IsLoaded
        {
            get;
        }

        DependencyObject TemplatedParent
        {
            get;
        }
    }

    /// <summary>
    /// Acts as a factory for FrameworkElementProxy and FrameworkContentElementProxy
    /// </summary>
    public abstract class FrameworkLevelElementFactory : InputLevelElement
    {
        public static new IFrameworkLevelElement FromElement(DependencyObject element)
        {
            IFrameworkLevelElement frameworkLevelElement = ZoomElementFactory.FromElement(element);
            if (frameworkLevelElement != null)
            {
                return frameworkLevelElement;
            }
            else if (element is FrameworkElement)
            {
                return new FrameworkElementProxy(element as FrameworkElement);
            }
            else if (element is FrameworkContentElement)
            {
                return new FrameworkContentElementProxy(element as FrameworkContentElement);
            }
            else
            {
                return null;
            }
        }

        protected FrameworkLevelElementFactory(DependencyObject proxiedObject)
            : base(proxiedObject)
        {
        }

        protected class FrameworkElementProxy : UIElementProxy, IFrameworkLevelElement
        {
            public FrameworkElementProxy(FrameworkElement proxied)
                : base(proxied) { }

            public event EventHandler Initialized
            {
                add
                {
                    this.Handle.Initialized += value;
                }

                remove
                {
                    this.Handle.Initialized -= value;
                }
            }

            public event RoutedEventHandler Loaded
            {
                add
                {
                    this.Handle.Loaded += value;
                }

                remove
                {
                    this.Handle.Loaded -= value;
                }
            }

            public event RoutedEventHandler Unloaded
            {
                add
                {
                    this.Handle.Unloaded += value;
                }

                remove
                {
                    this.Handle.Unloaded -= value;
                }
            }

            public bool IsInitialized => this.Handle.IsInitialized;

            public bool IsLoaded => this.Handle.IsLoaded;

            public DependencyObject TemplatedParent => this.Handle.TemplatedParent;

            private FrameworkElement Handle => this.Proxied as FrameworkElement;
        }

        protected class FrameworkContentElementProxy : ContentElementProxy, IFrameworkLevelElement
        {
            public FrameworkContentElementProxy(FrameworkContentElement proxied)
                : base(proxied) { }

            public event EventHandler Initialized
            {
                add
                {
                    this.Handle.Initialized += value;
                }

                remove
                {
                    this.Handle.Initialized -= value;
                }
            }

            public event RoutedEventHandler Loaded
            {
                add
                {
                    this.Handle.Loaded += value;
                }

                remove
                {
                    this.Handle.Loaded -= value;
                }
            }

            public event RoutedEventHandler Unloaded
            {
                add
                {
                    this.Handle.Unloaded += value;
                }

                remove
                {
                    this.Handle.Unloaded -= value;
                }
            }

            public bool IsInitialized => this.Handle.IsInitialized;

            public bool IsLoaded => this.Handle.IsLoaded;

            public DependencyObject TemplatedParent => this.Handle.TemplatedParent;

            private FrameworkContentElement Handle => this.Proxied as FrameworkContentElement;
        }
    }
}
