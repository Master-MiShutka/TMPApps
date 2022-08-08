namespace TMP.WORK.AramisChetchiki.Model
{
    using System;
    using System.Collections.Generic;
    using System.Windows.Input;

    public class HierarchicalItem : IDisposable
    {
        private bool disposedValue;
        private WeakReference<HierarchicalItem> parent;
        private WeakReference<ICommand> command;
        private WeakReference<object> commandParameter;
        private WeakReference<object> tag;

        public static uint InstancesCount { get; private set; }

        static HierarchicalItem()
        {
            InstancesCount = 0;
        }

        private ICollection<HierarchicalItem> items = null;

        public HierarchicalItem Parent
        {
            get
            {
                if (this.parent == null)
                    return null;

                if (this.parent.TryGetTarget(out HierarchicalItem item))
                    return item;
                else
                    return null;
            }

            set
            {
                this.parent = new(value!);
            }
        }

        public string Name { get; set; }

        public ICommand Command
        {
            get
            {
                if (this.command == null)
                    return null;

                if (this.command.TryGetTarget(out ICommand item))
                    return item;
                else
                    return null;
            }

            set
            {
                this.command = new(value!);
            }
        }

        public object CommandParameter
        {
            get
            {
                if (this.commandParameter == null)
                    return null;

                if (this.commandParameter.TryGetTarget(out object item))
                    return item;
                else
                    return null;
            }

            set
            {
                this.commandParameter = new(value!);
            }
        }

        public string Description { get; set; }

        public object Tag
        {
            get
            {
                if (this.tag == null)
                    return null;

                if (this.tag.TryGetTarget(out object item))
                    return item;
                else
                    return null;
            }

            set
            {
                this.tag = new(value!);
            }
        }

        public bool IsCheckable { get; private set; }

        public IEnumerable<HierarchicalItem> Items
        {
            get => this.items;
            set
            {
                if (value == null)
                {
                    return;
                }

                this.items = new List<HierarchicalItem>();
                foreach (HierarchicalItem item in value)
                {
                    item.Parent = this;
                    this.items.Add(item);
                }
            }
        }

        public HierarchicalItem()
        {
            InstancesCount++;
        }

        public HierarchicalItem(string name, ICommand action, bool isCheckable = false)
            : this()
        {
            this.Name = name;
            this.Command = action;
            this.IsCheckable = isCheckable;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    if (this.Items != null)
                    {
                        foreach (var child in this.Items)
                        {
                            child.Dispose();
                        }
                    }

                    if (this.CommandParameter != null && this.CommandParameter is IDisposable disposable1)
                    {
                        disposable1.Dispose();
                    }

                    if (this.Tag != null && this.Tag is IDisposable disposable2)
                    {
                        disposable2.Dispose();
                    }

                    if (this.Command != null && this.Command is IDisposable disposable3)
                    {
                        disposable3.Dispose();
                    }
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                this.disposedValue = true;

                InstancesCount--;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~HierarchicalItem()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            this.Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
