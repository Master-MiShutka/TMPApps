using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Threading;
using System.ComponentModel;

namespace TMP.Work.Emcos
{
    public class AsyncObservableCollection<T> : ObservableCollection<T>
    {
        private SynchronizationContext _synchronizationContext = SynchronizationContext.Current;

        public AsyncObservableCollection()
        {
        }

        public AsyncObservableCollection(IEnumerable<T> list)
            : base(list)
        {
        }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
#if DEBUG
            NotifyCollectionChangedEventArgs pcea = (NotifyCollectionChangedEventArgs)e;
            App.LOG.Log(String.Format(
                "{0}\t{1}\t{2}",
                "AsyncObservableCollection",
                "OnCollectionChanged",
                pcea.ToString()), Common.Logger.Category.Info, Common.Logger.Priority.None);
#endif
            if (SynchronizationContext.Current == _synchronizationContext)
            {
                // Execute the CollectionChanged event on the current thread
                RaiseCollectionChanged(e);
            }
            else
            {
                // Raises the CollectionChanged event on the creator thread
                _synchronizationContext.Send(RaiseCollectionChanged, e);
            }
        }

        private void RaiseCollectionChanged(object param)
        {
#if DEBUG
            NotifyCollectionChangedEventArgs pcea = (NotifyCollectionChangedEventArgs)param;
            App.LOG.Log(String.Format(
                "{0}\t{1}\t{2}",
                "AsyncObservableCollection",
                "RaiseCollectionChanged",
                pcea.ToString()), Common.Logger.Category.Info, Common.Logger.Priority.None);
#endif
            // We are in the creator thread, call the base implementation directly
            base.OnCollectionChanged((NotifyCollectionChangedEventArgs)param);
        }

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
#if DEBUG
            PropertyChangedEventArgs pcea = (PropertyChangedEventArgs)e;
            App.LOG.Log(String.Format(
                "{0}\t{1}\t{2}",
                "AsyncObservableCollection",
                "OnPropertyChanged",
                pcea.PropertyName), Common.Logger.Category.Info, Common.Logger.Priority.None);
#endif
            if (SynchronizationContext.Current == _synchronizationContext)
            {
                // Execute the PropertyChanged event on the current thread
                RaisePropertyChanged(e);
            }
            else
            {
                // Raises the PropertyChanged event on the creator thread
                _synchronizationContext.Send(RaisePropertyChanged, e);
            }
        }

        private void RaisePropertyChanged(object param)
        {
#if DEBUG
            PropertyChangedEventArgs pcea = (PropertyChangedEventArgs)param;
            App.LOG.Log(String.Format(
                "{0}\t{1}\t{2}",
                "AsyncObservableCollection",
                "RaisePropertyChanged",
                pcea.PropertyName), Common.Logger.Category.Info, Common.Logger.Priority.None);
#endif
            // We are in the creator thread, call the base implementation directly
            base.OnPropertyChanged((PropertyChangedEventArgs)param);
        }
    }

}
