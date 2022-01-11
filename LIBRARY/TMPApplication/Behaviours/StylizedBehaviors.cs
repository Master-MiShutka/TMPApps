namespace TMPApplication.Behaviours
{
    using System.ComponentModel;
    using System.Windows;
    using Interactivity;

    public class StylizedBehaviors
    {
        public static readonly DependencyProperty BehaviorsProperty = DependencyProperty.RegisterAttached("Behaviors",
                                          typeof(StylizedBehaviorCollection),
                                          typeof(StylizedBehaviors),
                                          new FrameworkPropertyMetadata(null, OnPropertyChanged));

        public static StylizedBehaviorCollection GetBehaviors(DependencyObject uie)
        {
            return (StylizedBehaviorCollection)uie.GetValue(BehaviorsProperty);
        }

        public static void SetBehaviors(DependencyObject obj, StylizedBehaviorCollection value)
        {
            obj.SetValue(BehaviorsProperty, value);
        }

        private static void OnPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var element = obj as FrameworkElement;
            if (element == null)
            {
                return;
            }

            var newBehaviors = e.NewValue as StylizedBehaviorCollection;
            var oldBehaviors = e.OldValue as StylizedBehaviorCollection;
            if (newBehaviors == oldBehaviors)
            {
                return;
            }

            BehaviorCollection itemBehaviors = Interaction.GetBehaviors(element);

            element.Unloaded -= FrameworkElementUnloaded;

            if (oldBehaviors != null)
            {
                foreach (var behavior in oldBehaviors)
                {
                    int index = GetIndexOf(itemBehaviors, behavior);
                    if (index >= 0)
                    {
                        itemBehaviors.RemoveAt(index);
                    }
                }
            }

            if (newBehaviors != null)
            {
                foreach (var behavior in newBehaviors)
                {
                    int index = GetIndexOf(itemBehaviors, behavior);
                    if (index < 0)
                    {
                        var clone = (Behavior)behavior.Clone();
                        SetOriginalBehavior(clone, behavior);
                        itemBehaviors.Add(clone);
                    }
                }
            }

            if (itemBehaviors.Count > 0)
            {
                element.Unloaded += FrameworkElementUnloaded;
            }

            element.Dispatcher.ShutdownStarted += Dispatcher_ShutdownStarted;
        }

        private static void Dispatcher_ShutdownStarted(object sender, System.EventArgs e)
        {
        }

        private static void FrameworkElementUnloaded(object sender, RoutedEventArgs e)
        {
            // BehaviorCollection doesn't call Detach, so we do this
            var element = sender as FrameworkElement;
            if (element == null)
            {
                return;
            }

            BehaviorCollection itemBehaviors = Interaction.GetBehaviors(element);
            foreach (var behavior in itemBehaviors)
            {
                behavior.Detach();
            }

            element.Loaded += FrameworkElementLoaded;
        }

        private static void FrameworkElementLoaded(object sender, RoutedEventArgs e)
        {
            var element = sender as FrameworkElement;
            if (element == null)
            {
                return;
            }

            element.Loaded -= FrameworkElementLoaded;
            BehaviorCollection itemBehaviors = Interaction.GetBehaviors(element);
            foreach (var behavior in itemBehaviors)
            {
                behavior.Attach(element);
            }
        }

        private static int GetIndexOf(BehaviorCollection itemBehaviors, Behavior behavior)
        {
            int index = -1;

            Behavior originalBehavior = GetOriginalBehavior(behavior);

            for (int i = 0; i < itemBehaviors.Count; i++)
            {
                Behavior currentBehavior = itemBehaviors[i];
                if (currentBehavior == behavior || currentBehavior == originalBehavior)
                {
                    index = i;
                    break;
                }

                Behavior currentOrignalBehavior = GetOriginalBehavior(currentBehavior);
                if (currentOrignalBehavior == behavior || currentOrignalBehavior == originalBehavior)
                {
                    index = i;
                    break;
                }
            }

            return index;
        }

        private static readonly DependencyProperty OriginalBehaviorProperty
            = DependencyProperty.RegisterAttached("OriginalBehaviorInternal",
                                                  typeof(Behavior),
                                                  typeof(StylizedBehaviors),
                                                  new UIPropertyMetadata(null));

        private static Behavior GetOriginalBehavior(DependencyObject obj)
        {
            return obj.GetValue(OriginalBehaviorProperty) as Behavior;
        }

        private static void SetOriginalBehavior(DependencyObject obj, Behavior value)
        {
            obj.SetValue(OriginalBehaviorProperty, value);
        }
    }
}
