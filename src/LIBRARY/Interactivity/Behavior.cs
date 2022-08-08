namespace Interactivity
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Media.Animation;

    /// <summary>
    /// Encapsulates state information and zero or more ICommands into an attachable object.
    /// </summary>
    /// <remarks>This is an infrastructure class. Behavior authors should derive from Behavior&lt;T&gt; instead of from this class.</remarks>
    public abstract class Behavior : Animatable, IAttachedObject
    {
        private Type associatedType;
        private DependencyObject associatedObject;

        internal event EventHandler AssociatedObjectChanged;

        /// <summary>
        /// The type to which this behavior can be attached.
        /// </summary>
        protected Type AssociatedType
        {
            get
            {
                base.ReadPreamble();
                return this.associatedType;
            }
        }

        /// <summary>
        /// Gets the object to which this behavior is attached.
        /// </summary>
        protected DependencyObject AssociatedObject
        {
            get
            {
                base.ReadPreamble();
                return this.associatedObject;
            }
        }

        /// <summary>
        /// Gets the associated object.
        /// </summary>
        /// <value>The associated object.</value>
        DependencyObject IAttachedObject.AssociatedObject => this.AssociatedObject;

        internal Behavior(Type associatedType)
        {
            this.associatedType = associatedType;
        }

        /// <summary>
        /// Called after the behavior is attached to an AssociatedObject.
        /// </summary>
        /// <remarks>Override this to hook up functionality to the AssociatedObject.</remarks>
        protected virtual void OnAttached()
        {
        }

        /// <summary>
        /// Called when the behavior is being detached from its AssociatedObject, but before it has actually occurred.
        /// </summary>
        /// <remarks>Override this to unhook functionality from the AssociatedObject.</remarks>
        protected virtual void OnDetaching()
        {
        }

        protected override Freezable CreateInstanceCore()
        {
            Type type = base.GetType();
            return (Freezable)Activator.CreateInstance(type);
        }

        private void OnAssociatedObjectChanged()
        {
            if (this.AssociatedObjectChanged != null)
            {
                this.AssociatedObjectChanged(this, new EventArgs());
            }
        }

        /// <summary>
        /// Attaches to the specified object.
        /// </summary>
        /// <param name="dependencyObject">The object to attach to.</param>
        /// <exception cref="T:System.InvalidOperationException">The Behavior is already hosted on a different element.</exception>
        /// <exception cref="T:System.InvalidOperationException">dependencyObject does not satisfy the Behavior type constraint.</exception>
        public void Attach(DependencyObject dependencyObject)
        {
            if (dependencyObject != this.AssociatedObject)
            {
                if (this.AssociatedObject != null)
                {
                    throw new InvalidOperationException("An instance of a Behavior cannot be attached to more than one object at a time");
                }

                if (dependencyObject != null && !this.AssociatedType.IsAssignableFrom(dependencyObject.GetType()))
                {
                    throw new InvalidOperationException(string.Format("Cannot attach type {0} to type {1}. Instances of type {0} can only be attached to objects of type {2}", new object[]
                    {
                        base.GetType().Name,
                        dependencyObject.GetType().Name,
                        this.AssociatedType.Name,
                    }));
                }

                base.WritePreamble();
                this.associatedObject = dependencyObject;
                base.WritePostscript();
                this.OnAssociatedObjectChanged();
                this.OnAttached();
            }
        }

        /// <summary>
        /// Detaches this instance from its associated object.
        /// </summary>
        public void Detach()
        {
            this.OnDetaching();
            base.WritePreamble();
            this.associatedObject = null;
            base.WritePostscript();
            this.OnAssociatedObjectChanged();
        }
    }

    /// <summary>
    /// Encapsulates state information and zero or more ICommands into an attachable object.
    /// </summary>
    /// <typeparam name="T">The type the <see cref="T:System.Windows.Interactivity.Behavior`1" /> can be attached to.</typeparam>
    /// <remarks>
    /// 	Behavior is the base class for providing attachable state and commands to an object.
    /// 	The types the Behavior can be attached to can be controlled by the generic parameter.
    /// 	Override OnAttached() and OnDetaching() methods to hook and unhook any necessary handlers
    /// 	from the AssociatedObject.
    /// </remarks>
    public abstract class Behavior<T> : Behavior
        where T : DependencyObject
    {
        /// <summary>
        /// Gets the object to which this <see cref="T:System.Windows.Interactivity.Behavior`1" /> is attached.
        /// </summary>
        protected new T AssociatedObject => (T)(object)base.AssociatedObject;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Windows.Interactivity.Behavior`1" /> class.
        /// </summary>
        protected Behavior()
            : base(typeof(T))
        {
        }
    }
}