namespace WpfMouseWheel.Maths
{
    using System;

    /// <summary>
    /// Filters high frequencies of an input signal
    /// </summary>
    public class LowPassFilter
    {
        public LowPassFilter()
        {
        }

        public LowPassFilter(double lifetime)
        {
            this.Lifetime = lifetime;
        }

        /// <summary>
        /// Gets or sets the lifetime of the filter
        /// </summary>
        public double Lifetime
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the current input signal value or increment
        /// </summary>
        public double Input
        {
            get; private set;
        }

        /// <summary>
        /// Gets the current output signal value or increment
        /// </summary>
        public double Output
        {
            get; private set;
        }

        /// <summary>
        /// Inputs a new signal increment.
        /// </summary>
        public void NewInputDelta(double delta)
        {
            this.Input += delta;
        }

        /// <summary>
        /// Computes next output signal increment
        /// </summary>
        public double NextOutputDelta(double t, double dtMin)
        {
            var dt = double.IsNaN(this._t0) ? dtMin : t - this._t0;
            this._t0 = t;
            double gain = Math.Min(1.0, dt / this.Lifetime);
            this.Output += gain * (this.Input - this.Output);
            this.Input = 0;
            return this.Output;
        }

        private double _t0 = double.NaN;
    }
}
