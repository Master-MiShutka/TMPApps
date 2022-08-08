namespace WpfMouseWheel.Windows.Input
{
    using System;
    using System.Linq;
    using System.Windows;
    using System.Windows.Input;
    using WpfMouseWheel.Windows.MotionFlow;

    public interface IMouseWheelClient : IMouseWheelInputListener, IMotionTarget, IDisposable
    {
        IMouseWheelController Controller
        {
            get;
        }

        double MotionIncrement
        {
            get;
        }

        MouseWheelSmoothing Smoothing
        {
            get;
        }

        IInputElement ExitElement
        {
            get;
        }

        bool IsActive(MouseWheelInputEventArgs e);

        void Unload();
    }

    public abstract class MouseWheelClient : IMouseWheelClient
    {
        public MouseWheelClient(IMouseWheelController controller)
        {
            if (controller == null)
            {
                throw new ArgumentNullException("controller");
            }

            this.Controller = controller;
        }

        public IMouseWheelController Controller
        {
            get;
        }

        public virtual double MotionIncrement => 1.0;

        public virtual MouseWheelSmoothing Smoothing
        {
            get => MouseWheelSmoothing.None;

            protected set => throw new NotImplementedException();
        }

        public abstract ModifierKeys Modifiers
        {
            get;
        }

        public IInputElement ExitElement
        {
            get
            {
                if (this._exitElement == null)
                {
                    this._exitElement = this.GetExitElement();
                }

                return this._exitElement;
            }
        }

        public virtual bool IsActive(MouseWheelInputEventArgs e)
        {
            this.EnsureLoaded();
            return this.Modifiers == Keyboard.Modifiers;
        }

        public void Unload()
        {
            if (this._loaded)
            {
                this._loaded = false;
                this.OnUnloading();
                this.InvalidateBehavior();
            }
        }

        public void OnPreviewInput(object sender, MouseWheelInputEventArgs e)
        {
            this.Behavior.OnPreviewInput(sender, e);
        }

        public void OnInput(object sender, MouseWheelInputEventArgs e)
        {
            this.Behavior.OnInput(sender, e);
        }

        public virtual bool CanMove(IMotionInfo info, object context)
        {
            return true;
        }

        public virtual double Coerce(IMotionInfo info, object context, double delta)
        {
            return delta;
        }

        public virtual void Move(IMotionInfo info, object context, double delta)
        {
        }

        public virtual void Dispose()
        {
            this.Unload();
        }

        protected IMouseWheelInputListener Behavior
        {
            get
            {
                if (this._behavior == null)
                {
                    this.EnsureLoaded();
                    this._behavior = this.CreateBehavior();
                }

                return this._behavior;
            }
        }

        protected bool Enhanced
        {
            get => this._enhanced;
            set
            {
                if (this._enhanced == value)
                    return;
                this._enhanced = value;
                this.InvalidateBehavior();
            }
        }

        protected void InvalidateBehavior()
        {
            this.DisposeBehavior();
            this._behavior = null;
        }

        protected abstract IMouseWheelInputListener CreateBehavior();

        protected virtual IInputElement GetExitElement()
        {
            return this.Controller.Element.GetVisualAncestors().OfType<IInputElement>().FirstOrDefault();
        }

        protected virtual void OnLoading()
        {
            this._enhanced = MouseWheel.GetEnhanced(this.Controller.Element);
            MouseWheel.EnhancedProperty.AddValueChanged(this.Controller.Element, this.OnEnhancedChanged);
        }

        protected virtual void OnUnloading()
        {
            MouseWheel.EnhancedProperty.RemoveValueChanged(this.Controller.Element, this.OnEnhancedChanged);
        }

        protected void EnsureLoaded()
        {
            if (!this._loaded)
            {
                this._loaded = true;
                this.OnLoading();
            }
        }

        private void DisposeBehavior()
        {
            if (this._behavior is IDisposable)
                (this._behavior as IDisposable).Dispose();
        }

        private void OnEnhancedChanged(object sender, EventArgs e)
        {
            this.Enhanced = MouseWheel.GetEnhanced(sender as DependencyObject);
        }

        private IMouseWheelInputListener _behavior;
        private bool _enhanced;
        private bool _loaded;
        private IInputElement _exitElement;

    }
}
