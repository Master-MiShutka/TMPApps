﻿namespace WpfMouseWheel.Maths
{
    using System;

    /// <summary>
    /// Linearizes a step function by extrapolating with a bounded linear function
    /// </summary>
    public class DifferentialLinearFilter
    {

        /// <summary>
        /// Gets the slope of the extrapolated linear function.
        /// </summary>
        public double Slope => this._se;

        /// <summary>
        /// Gets the bounding value of the extrapolated linear function
        /// </summary>
        public double YMax => this._ymax;

        /// <summary>
        /// Inputs a new step increment.
        /// </summary>
        /// <param name="x1">The abscissa where the step function changes</param>
        /// <param name="dy1">The step increment</param>
        /// <param name="smin">The minimal slope modulus</param>
        /// <remarks>
        /// Updates the slope and the bounding value of the the extrapolated linear function.
        /// </remarks>
        public void NewInputDelta(double x1, double dy1, double smin)
        {
            if (DoubleEx.IsZero(dy1))
            {
                throw new ArgumentNullException("dy1");
            }

            double sm;
            if (double.IsNaN(this._x))
            {
                sm = smin;
                this._ymax = dy1;
            }
            else
            {
                var dx1 = x1 - this._x;
                var y1 = this.CoerceY(this._se * dx1);
                sm = Math.Max(smin, Math.Abs(this._ymax / dx1));
                this._ymax += dy1 - y1;

                // sub-sampling correction
                this._dyc += y1 - this._ys;
                this._ys = 0;

                // Debug.WriteLine(string.Format("dx={0,7:F2}[ms], dy1={1,7:F2}, y1={2,7:F2}, ymax={3,7:F2}, slope={4,7:F2}", dx1 * 1000, dy1, y1, _ymax, _se));
            }

            this._se = sm * Math.Sign(this._ymax);
            this._x = x1;
        }

        /// <summary>
        /// Subsamples the extrapolated function.
        /// </summary>
        /// <param name="xs1">The abscissa where the function is subsampled</param>
        /// <returns>The subsampling increment</returns>
        /// <remarks>
        /// The subsampling increment is bounded to <see cref="YMax"/>.
        /// </remarks>
        public double NextOutputDelta(double xs1)
        {
            var dxs1 = xs1 - this._x;
            var ys1 = this.CoerceY(this._se * dxs1);
            var dys1 = ys1 - this._ys + this._dyc;
            this._dyc = 0;
            this._ys = ys1;

            // Debug.WriteLine(string.Format("dxs={0,7:F2}[ms], dys1={1,7:F2}, ys1={2,7:F2}, slope={3,7:F2}", dxs1 * 1000, dys1, ys1, ys1 / dxs1));
            return dys1;
        }

        public override string ToString()
        {
            return string.Format("t={0,6:F3}, se={1,6:F2}, ymax={2,5:F2}, dyc={3,5:F2}, ys={4,5:F2}", this._x, this._se, this._ymax, this._dyc, this._ys);
        }

        private double CoerceY(double y1)
        {
            return Math.Sign(this._ymax) > 0 ? Math.Min(this._ymax, y1) : Math.Max(this._ymax, y1);
        }

        /// <summary>
        /// Identifies the abscissa where the step function changes
        /// </summary>
        private double _x = double.NaN;

        /// <summary>
        /// Identifies the slope of the extrapolated linear function
        /// </summary>
        private double _se;

        /// <summary>
        /// Identifies the bounding value of the extrapolated linear function
        /// </summary>
        private double _ymax;

        /// <summary>
        /// Identifies the subsampling correction
        /// </summary>
        private double _dyc;

        /// <summary>
        /// Identifies the current subsampling value
        /// </summary>
        private double _ys;
    }
}
