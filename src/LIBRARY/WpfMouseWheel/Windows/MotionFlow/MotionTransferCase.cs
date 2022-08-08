namespace WpfMouseWheel.Windows.MotionFlow
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Windows;

    public interface IMotionTransferCase : IMotionInput, IEnumerable<IMotionTransfer>, IMotionTransferOutput
    {
    }

    public interface INativeMotionTransferCase : INativeMotionInput, IEnumerable<INativeMotionTransfer>, INativeMotionTransferOutput
    {
    }

    public abstract class MotionTransferCaseBase : MotionElement, IMotionTransferCase
    {
        public IMotionInfo MotionInfo => this.First().MotionInfo;

        public double Remainder => this.Sum(item => item.Remainder);

        public void Transmit(IMotionInfo info, double delta, IMotionOutput source)
        {
            foreach (var item in this)
                item.Transmit(info, delta, source);
        }

        public void OnCoupledTransfer(IMotionInfo info, double delta, IMotionTransferOutput source)
        {
            foreach (var item in this)
                item.OnCoupledTransfer(info, delta, source);
        }

        public void Reset()
        {
            foreach (var item in this)
                item.Reset();
        }

        public bool Transfer(IMotionTarget target, object context)
        {
            return this.All(item => item.Transfer(target, context));
        }

        public abstract IEnumerator<IMotionTransfer> GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }

    public abstract class NativeMotionTransferCaseBase : ContentElement, INativeMotionTransferCase
    {
        public IMotionInfo MotionInfo => this.First().MotionInfo;

        public double Remainder => this.Sum(item => item.Remainder);

        public int NativeRemainder => this.Sum(item => item.NativeRemainder);

        public void Transmit(IMotionInfo info, int nativeDelta, INativeMotionOutput source)
        {
            foreach (var item in this)
                item.Transmit(info, nativeDelta, source);
        }

        public void OnCoupledTransfer(IMotionInfo info, int nativeDelta, INativeMotionTransferOutput source)
        {
            foreach (var item in this)
                item.OnCoupledTransfer(info, nativeDelta, source);
        }

        public void Reset()
        {
            foreach (var item in this)
                item.Reset();
        }

        public bool Transfer(IMotionTarget target, object context)
        {
            return this.All(item => item.Transfer(target, context));
        }

        public bool Transfer(INativeMotionTarget target, object context)
        {
            return this.All(item => item.Transfer(target, context));
        }

        public abstract IEnumerator<INativeMotionTransfer> GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }

    public class MotionTransferCase : MotionTransferCaseBase
    {
        public void Add(IMotionTransfer item)
        {
            this._items.Add(item);
            item.SetParent(this);
        }

        public bool Remove(IMotionTransfer item)
        {
            bool removed = this._items.Remove(item);
            if (removed)
                item.SetParent(null);
            return removed;
        }

        public override IEnumerator<IMotionTransfer> GetEnumerator()
        {
            return this._items.GetEnumerator();
        }

        private readonly List<IMotionTransfer> _items = new List<IMotionTransfer>();
    }

    public class NativeMotionTransferCase : NativeMotionTransferCaseBase
    {
        public void Add(INativeMotionTransfer item)
        {
            this._items.Add(item);
            item.SetParent(this);
        }

        public bool Remove(INativeMotionTransfer item)
        {
            bool removed = this._items.Remove(item);
            if (removed)
                item.SetParent(null);
            return removed;
        }

        public override IEnumerator<INativeMotionTransfer> GetEnumerator()
        {
            return this._items.GetEnumerator();
        }

        private readonly List<INativeMotionTransfer> _items = new List<INativeMotionTransfer>();
    }

    public class NativeCoupledMotionTransferCase : NativeMotionTransferCase
    {
        public NativeCoupledMotionTransferCase()
        {
            this.AddHandler(NativeMotionTransfer.TransferedEvent, new NativeMotionTransferEventHandler(this.OnMotionTransfered));
        }

        private void OnMotionTransfered(object sender, NativeMotionTransferEventArgs e)
        {
            var source = e.Source as INativeMotionTransferOutput;
            Debug.Assert(source != null);
            foreach (var item in this)
                item.OnCoupledTransfer(e.MotionInfo, e.NativeDelta, source);
        }
    }
}
