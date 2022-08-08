namespace Interactivity
{
    using System;
    using System.Windows;

    /// <summary>
    /// Static class that owns the Triggers and Behaviors attached properties. Handles propagation of AssociatedObject change notifications.
    /// </summary>
    public static class Interaction
    {
        /// <summary>
        /// Gets the associated with a specified object.
        /// </summary>
        /// <param name="obj">The object from which to retrieve the <see cref="T:System.Windows.Interactivity.BehaviorCollection" />.</param>
        /// <returns>A <see cref="T:System.Windows.Interactivity.BehaviorCollection" /> containing the behaviors associated with the specified object.</returns>
        public static BehaviorCollection GetBehaviors(DependencyObject obj)
        {
            BehaviorCollection behaviorCollection = (BehaviorCollection)obj.GetValue(Interaction.BehaviorsProperty);
            if (behaviorCollection == null)
            {
                behaviorCollection = new BehaviorCollection();
                obj.SetValue(Interaction.BehaviorsProperty, behaviorCollection);
            }

            return behaviorCollection;
        }

        /// <summary>
        /// This property is used as the internal backing store for the public Behaviors attached property.
        /// </summary>
        /// <remarks>
        /// This property is not exposed publicly. This forces clients to use the GetBehaviors and SetBehaviors methods to access the
        /// collection, ensuring the collection exists and is set before it is used.
        /// </remarks>
        private static readonly DependencyProperty BehaviorsProperty = DependencyProperty.RegisterAttached("ShadowBehaviors", typeof(BehaviorCollection), typeof(Interaction), new FrameworkPropertyMetadata(new PropertyChangedCallback(Interaction.OnBehaviorsChanged)));

        /// <exception cref="T:System.InvalidOperationException">Cannot host the same BehaviorCollection on more than one object at a time.</exception>
        private static void OnBehaviorsChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            BehaviorCollection behaviorCollection = (BehaviorCollection)args.OldValue;
            BehaviorCollection behaviorCollection2 = (BehaviorCollection)args.NewValue;
            if (behaviorCollection != behaviorCollection2)
            {
                if (behaviorCollection != null && ((IAttachedObject)behaviorCollection).AssociatedObject != null)
                {
                    behaviorCollection.Detach();
                }

                if (behaviorCollection2 != null && obj != null)
                {
                    if (((IAttachedObject)behaviorCollection2).AssociatedObject != null)
                    {
                        throw new InvalidOperationException("Cannot set the same BehaviorCollection on multiple objects.");
                    }

                    behaviorCollection2.Attach(obj);
                }
            }
        }
    }
}