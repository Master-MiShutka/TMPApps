namespace WpfMouseWheel.Windows.Input
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using WpfMouseWheel.Maths;
    using WpfMouseWheel.Windows.MotionFlow;

    public interface IMouseWheelTransferCase : INativeMotionTransferCase
    {
        IMouseWheelShaft this[double debouncingCellCount] { get; }

        IMouseWheelShaft this[int debouncingCellCount] { get; }

        IMouseWheelShaft ActiveShaft
        {
            get; set;
        }
    }

    public class MouseWheelSingleShaftTransferCase : MouseWheelShaft, IMouseWheelTransferCase
    {
        public MouseWheelSingleShaftTransferCase(int resolution)
            : base(resolution) { }

        public IMouseWheelShaft ActiveShaft
        {
            get => this;
            set
            {
                if (value != this)
                    throw new InvalidOperationException();
            }
        }

        public IMouseWheelShaft this[double resolution] => this;

        public IMouseWheelShaft this[int resolution] => this;

        public IEnumerator<INativeMotionTransfer> GetEnumerator()
        {
            yield return this;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }

    public class MouseWheelMultiShaftTransferCase : NativeCoupledMotionTransferCase, IMouseWheelTransferCase
    {
        private readonly MouseWheel _wheel;

        /// <summary>
        /// Identifies a dictionary of periodic debouncing functions where the key represents the count of debouncing cells per period
        /// </summary>
        private readonly Dictionary<int, Int32DifferentialFunctionPatternModulator> _debouncingFunctions = new Dictionary<int, Int32DifferentialFunctionPatternModulator>();
        private readonly Dictionary<int, IMouseWheelShaft> _shafts = new Dictionary<int, IMouseWheelShaft>();

        public MouseWheelMultiShaftTransferCase(MouseWheel wheel)
        {
            this._wheel = wheel;
            this.InitializeDebouncingFunctions();
        }

        public IMouseWheelShaft this[double debouncingCellCount] => this.GetOrAddShaft(this.MatchDebouncingCellCount(debouncingCellCount));

        public IMouseWheelShaft this[int debouncingCellCount] => this.GetOrAddShaft(this.MatchDebouncingCellCount(debouncingCellCount));

        public IMouseWheelShaft ActiveShaft
        {
            get; set;
        }

        private IOrderedEnumerable<int> DebouncingCellCountsOrderByDescending => this._debouncingFunctions.Keys.OrderByDescending(i => i);

        private int MatchDebouncingCellCount(double desiredCount)
        {
            return this.MatchDebouncingCellCount((int)Math.Round(desiredCount));
        }

        private int MatchDebouncingCellCount(int desiredCount)
        {
            if (desiredCount < 0)
            {
                // auto debouncing
                if (this._wheel.NativeNotchFrequency % this._wheel.NativeResolutionFrequency != 0)

                    // no debouncing if resolution not integral
                    return 0;
                else

                    // the most granular debouncing
                    return this.DebouncingCellCountsOrderByDescending.FirstOrDefault();
            }
            else if (desiredCount <= 1)
            {

                // no debouncing or single debouncing
                return desiredCount;
            }
            else
            {

                // match the greater number of debouncing cells that is an exact divisor of the desired count
                // no debouncing if not an exact divisor 
                return this.DebouncingCellCountsOrderByDescending.FirstOrDefault(key => desiredCount % key == 0);
            }
        }

        private Int32DifferentialFunctionPatternModulator CreateDebouncingFunction(int hysteronCount)
        {
            var hysterons = Int32HysteronGenerator.CreateFunctions(hysteronCount,
              this._wheel.NativeNotchFrequency, this._wheel.NativeResolutionFrequency, 2, 1);

            var pattern = Int32FunctionSummator.CreateFunction(hysterons);
            if (pattern == null)
                return null;
            return new Int32DifferentialFunctionPatternModulator(x => x / hysteronCount, pattern, this._wheel.NativeNotchFrequency);
        }

        private void InitializeDebouncingFunctions()
        {
            for (int hysteronCount = 1; hysteronCount <= this._wheel.NativeNotchFrequency / this._wheel.NativeResolutionFrequency; ++hysteronCount)
            {
                var f = this.CreateDebouncingFunction(hysteronCount);
                if (f == null)
                    break;
                this._debouncingFunctions.Add(hysteronCount, f);
            }
        }

        private IMouseWheelShaft GetOrAddShaft(int resolution)
        {
            IMouseWheelShaft shaft;
            if (!this._shafts.TryGetValue(resolution, out shaft))
            {
                if (resolution == 0)
                    shaft = this.CreateDirectShaft();
                else
                    shaft = this.CreateDebouncedShaft(resolution);
                this._shafts[resolution] = shaft;
            }

            return shaft;
        }

        private IMouseWheelShaft CreateDirectShaft()
        {
            var transfer = new MouseWheelShaft(0);
            transfer.Name = transfer.Id.ToString("'D'00");
            this.Add(transfer);
            return transfer;
        }

        private IMouseWheelShaft CreateDebouncedShaft(int resolution)
        {
            var debouncing = new NativeDebouncedMotionTransform(this._debouncingFunctions[resolution]);
            var transfer = new MouseWheelShaft(resolution);
            debouncing.Next = transfer;
            var debouncedTransfer = new NativeMotionTransferGroup(debouncing, transfer);
            this.Add(debouncedTransfer);
            return transfer;
        }
    }
}
