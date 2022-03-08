namespace TMP.Shared
{
    using System;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.Windows.Threading;

    public class MTObservableCollection<T> : ObservableCollection<T>
    {
        public override event NotifyCollectionChangedEventHandler CollectionChanged;

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            NotifyCollectionChangedEventHandler collectionChanged = this.CollectionChanged;
            if (collectionChanged != null)
            {
                foreach (NotifyCollectionChangedEventHandler nh in collectionChanged.GetInvocationList())
                {
                    if (nh.Target is DispatcherObject dispObj)
                    {
                        Dispatcher dispatcher = dispObj.Dispatcher;
                        if (dispatcher != null && !dispatcher.CheckAccess())
                        {
                            dispatcher.BeginInvoke(() => nh.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset)), DispatcherPriority.DataBind);
                            continue;
                        }
                    }

                    nh.Invoke(this, e);
                }
            }
        }
    }
}
