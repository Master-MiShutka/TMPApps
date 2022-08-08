namespace WpfMouseWheel.Windows
{
    using System;
    using System.Windows;
    using System.Windows.Input;

    /// <summary>
    /// Represents a common interface for ContentElement, UIElement and UIElement3D classes
    /// </summary>
    public interface IInputLevelElement : IEquatable<IInputLevelElement>, IEquatable<DependencyObject>
    {
        event MouseWheelEventHandler PreviewMouseWheel;

        event MouseWheelEventHandler MouseWheel;

        DependencyObject Proxied
        {
            get;
        }

        InputBindingCollection InputBindings
        {
            get;
        }

        void RaiseEvent(RoutedEventArgs e);

        void AddHandler(RoutedEvent routedEvent, Delegate handler);

        void RemoveHandler(RoutedEvent routedEvent, Delegate handler);
    }

    /// <summary>
    /// Acts as a base class and factory for ContentElementProxy, UIElementProxy and UIElement3DProxy
    /// </summary>
    public abstract class InputLevelElement : IEquatable<IInputLevelElement>, IEquatable<DependencyObject>
    {
        public static IInputLevelElement FromElement(DependencyObject element)
        {
            IInputLevelElement inputLevelElement = FrameworkLevelElementFactory.FromElement(element);
            if (inputLevelElement != null)
            {
                return inputLevelElement;
            }

            if (element is UIElement)
            {
                return new UIElementProxy(element as UIElement);
            }
            else if (element is ContentElement)
            {
                return new ContentElementProxy(element as ContentElement);
            }
            else if (element is UIElement3D)
            {
                return new UIElement3DProxy(element as UIElement3D);
            }
            else
            {
                return null;
            }
        }

        public DependencyObject Proxied
        {
            get;
        }

        public override string ToString()
        {
            return this.Proxied.ToString();
        }

        public override int GetHashCode()
        {
            return this.Proxied.GetHashCode();
        }

        public override sealed bool Equals(object obj)
        {
            if (obj is IInputLevelElement)
            {
                return this.Equals(obj as IInputLevelElement);
            }
            else
            {
                return this.Equals(obj as DependencyObject);
            }
        }

        public bool Equals(IInputLevelElement other)
        {
            if (object.ReferenceEquals(this, other))
            {
                return true;
            }

            if (object.ReferenceEquals(other, null))
            {
                return false;
            }

            return object.ReferenceEquals(this.Proxied, other.Proxied);
        }

        public bool Equals(DependencyObject other)
        {
            return object.ReferenceEquals(this.Proxied, other);
        }

        protected InputLevelElement(DependencyObject proxied)
        {
            this.Proxied = proxied;
        }

        protected class UIElementProxy : InputLevelElement, IInputLevelElement
        {
            public UIElementProxy(UIElement proxied)
                : base(proxied) { }

            public event MouseWheelEventHandler PreviewMouseWheel
            {
                add
                {
                    this.Handle.PreviewMouseWheel += value;
                }

                remove
                {
                    this.Handle.PreviewMouseWheel -= value;
                }
            }

            public event MouseWheelEventHandler MouseWheel
            {
                add
                {
                    this.Handle.MouseWheel += value;
                }

                remove
                {
                    this.Handle.MouseWheel -= value;
                }
            }

            public InputBindingCollection InputBindings => this.Handle.InputBindings;

            public void RaiseEvent(RoutedEventArgs e)
            {
                this.Handle.RaiseEvent(e);
            }

            public void AddHandler(RoutedEvent routedEvent, Delegate handler)
            {
                this.Handle.AddHandler(routedEvent, handler);
            }

            public void RemoveHandler(RoutedEvent routedEvent, Delegate handler)
            {
                this.Handle.RemoveHandler(routedEvent, handler);
            }

            private UIElement Handle => this.Proxied as UIElement;
        }

        protected class ContentElementProxy : InputLevelElement, IInputLevelElement
        {
            public ContentElementProxy(ContentElement proxied)
                : base(proxied) { }

            public event MouseWheelEventHandler PreviewMouseWheel
            {
                add
                {
                    this.Handle.PreviewMouseWheel += value;
                }

                remove
                {
                    this.Handle.PreviewMouseWheel -= value;
                }
            }

            public event MouseWheelEventHandler MouseWheel
            {
                add
                {
                    this.Handle.MouseWheel += value;
                }

                remove
                {
                    this.Handle.MouseWheel -= value;
                }
            }

            public InputBindingCollection InputBindings => this.Handle.InputBindings;

            public void RaiseEvent(RoutedEventArgs e)
            {
                this.Handle.RaiseEvent(e);
            }

            public void AddHandler(RoutedEvent routedEvent, Delegate handler)
            {
                this.Handle.AddHandler(routedEvent, handler);
            }

            public void RemoveHandler(RoutedEvent routedEvent, Delegate handler)
            {
                this.Handle.RemoveHandler(routedEvent, handler);
            }

            private ContentElement Handle => this.Proxied as ContentElement;
        }

        protected class UIElement3DProxy : InputLevelElement, IInputLevelElement
        {
            public UIElement3DProxy(UIElement3D proxied)
                : base(proxied) { }

            public event MouseWheelEventHandler PreviewMouseWheel
            {
                add
                {
                    this.Handle.PreviewMouseWheel += value;
                }

                remove
                {
                    this.Handle.PreviewMouseWheel -= value;
                }
            }

            public event MouseWheelEventHandler MouseWheel
            {
                add
                {
                    this.Handle.MouseWheel += value;
                }

                remove
                {
                    this.Handle.MouseWheel -= value;
                }
            }

            public InputBindingCollection InputBindings => this.Handle.InputBindings;

            public void RaiseEvent(RoutedEventArgs e)
            {
                this.Handle.RaiseEvent(e);
            }

            public void AddHandler(RoutedEvent routedEvent, Delegate handler)
            {
                this.Handle.AddHandler(routedEvent, handler);
            }

            public void RemoveHandler(RoutedEvent routedEvent, Delegate handler)
            {
                this.Handle.RemoveHandler(routedEvent, handler);
            }

            private UIElement3D Handle => this.Proxied as UIElement3D;
        }
    }
}
