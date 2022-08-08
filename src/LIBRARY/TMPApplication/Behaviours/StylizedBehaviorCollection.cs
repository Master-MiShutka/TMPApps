namespace TMPApplication.Behaviours
{
    using System.Windows;
    using Interactivity;

    public class StylizedBehaviorCollection : FreezableCollection<Behavior>
    {
        protected override Freezable CreateInstanceCore()
        {
            return new StylizedBehaviorCollection();
        }
    }
}