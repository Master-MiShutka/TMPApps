namespace WpfMouseWheel.Maths
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    public class Int32FunctionTranslator : IFunction<int>
    {
        public Int32FunctionTranslator(Func<int, int> f, int dx = 0, int dy = 0)
        {
            this.OriginalFunction = f;
            this.TranslateX = dx;
            this.TranslateY = dy;
        }

        public Int32FunctionTranslator Clone()
        {
            return new Int32FunctionTranslator(this.OriginalFunction, this.TranslateX, this.TranslateY);
        }

        public int F(int x)
        {
            return this.OriginalFunction(x - this.TranslateX) + this.TranslateY;
        }

        public Func<int, int> OriginalFunction
        {
            get; set;
        }

        public int TranslateX
        {
            get; set;
        }

        public int TranslateY
        {
            get; set;
        }

        public void Translate(int dx, int dy)
        {
            this.TranslateX += dx;
            this.TranslateY += dy;
        }

        public override string ToString()
        {
            return string.Format("Translation=({0}, {1})", this.TranslateX, this.TranslateY);
        }
    }

    public class Int32FunctionSummator : IFunction<int>
    {
        public static Func<int, int> CreateFunction(IEnumerable<Func<int, int>> functions)
        {
            if (functions == null)
            {
                throw new ArgumentNullException("functions");
            }

            var functionArray = functions.ToArray();
            int count = functionArray.Length;
            if (count == 0)
            {
                return null;
            }
            else if (count == 1)
            {
                return functionArray[0];
            }
            else
            {
                return new Int32FunctionSummator(functionArray).F;
            }
        }

        public Int32FunctionSummator Clone()
        {
            return new Int32FunctionSummator(this.Functions);
        }

        public int F(int x)
        {
            return this.Functions.Sum(f => f(x));
        }

        public List<Func<int, int>> Functions
        {
            get; private set;
        }

        protected Int32FunctionSummator(IEnumerable<Func<int, int>> functions)
        {
            this.Functions = new List<Func<int, int>>(functions);
        }
    }

    public class Int32StateFunctionAdaptor : IFunction<int>
    {
        private int _state;
#pragma warning disable CS0246 // The type or namespace name 'RefFunc<,,>' could not be found (are you missing a using directive or an assembly reference?)
        private RefFunc<int, int, int> _f;
#pragma warning restore CS0246 // The type or namespace name 'RefFunc<,,>' could not be found (are you missing a using directive or an assembly reference?)

#pragma warning disable CS0246 // The type or namespace name 'RefFunc<,,>' could not be found (are you missing a using directive or an assembly reference?)
        public Int32StateFunctionAdaptor(RefFunc<int, int, int> f, int state = 0)
#pragma warning restore CS0246 // The type or namespace name 'RefFunc<,,>' could not be found (are you missing a using directive or an assembly reference?)
        {
            this._f = f;
            this._state = state;
        }

        [DebuggerStepThrough]
        public int F(int x)
        {
            return _f(ref this._state, x);
        }

        public int State
        {
            [DebuggerStepThrough]
            get => this._state;

            [DebuggerStepThrough]
            set => this._state = value;
        }

        public Int32StateFunctionAdaptor Clone()
        {
            return new Int32StateFunctionAdaptor(this._f, this._state);
        }
    }

    public class Int32FunctionToDifferentialFunctionAdaptor : IDifferentialFunction<int>
    {
        public Int32FunctionToDifferentialFunctionAdaptor(Func<int, int> function, int x = 0, int y = 0)
        {
            this.Function = function;
            this.X = x;
            this.Y = y;
        }

        public Func<int, int> Function
        {
            get; private set;
        }

        public int X
        {
            get; private set;
        }

        public int Y
        {
            get; private set;
        }

        public int DF(int dx)
        {
            var y = this.Function(this.X += dx);
            var dy = y - this.Y;
            this.Y = y;
            return dy;
        }

        public Int32FunctionToDifferentialFunctionAdaptor Clone()
        {
            return new Int32FunctionToDifferentialFunctionAdaptor(this.Function, this.X, this.Y);
        }
    }

    public class Int32DifferentialFunctionToFunctionAdaptor : IFunction<int>
    {
        public Int32DifferentialFunctionToFunctionAdaptor(Func<int, int> differentialFunction, int x = 0, int y = 0)
        {
            this.DifferentialFunction = differentialFunction;
            this.X = x;
            this.Y = y;
        }

        public Func<int, int> DifferentialFunction
        {
            get; private set;
        }

        public int X
        {
            get; private set;
        }

        public int Y
        {
            get; private set;
        }

        public int F(int x)
        {
            var dx = x - this.X;
            this.X = x;
            return this.Y += this.DifferentialFunction(dx);
        }

        public Int32DifferentialFunctionToFunctionAdaptor Clone()
        {
            return new Int32DifferentialFunctionToFunctionAdaptor(this.DifferentialFunction, this.X, this.Y);
        }
    }

    public class Int32DifferentialFunctionPatternModulator : IDifferentialFunction<int>
    {
        private readonly Int32FunctionTranslator _carrier;
        private int _patternHeight;
        private int _x, _y;

        public Int32DifferentialFunctionPatternModulator(Func<int, int> carrier, Func<int, int> pattern, int period)
          : this(new Int32FunctionTranslator(carrier), pattern, period)
        {
        }

        public Int32DifferentialFunctionPatternModulator(Int32FunctionTranslator carrier, Func<int, int> pattern, int period)
        {
            this._carrier = carrier;
            this._patternHeight = carrier.F(period);
            this.Pattern = pattern;
            this.Period = period;
        }

        public int DF(int dx)
        {
            if (dx == 0)
                return 0;
            else if (dx < 0)
                return this.NegativeMove(dx);
            else
                return this.PositiveMove(dx);
        }

        public Func<int, int> Carrier => this._carrier.F;

        public Func<int, int> Pattern
        {
            get; private set;
        }

        public int Period
        {
            get; private set;
        }

        public Int32DifferentialFunctionPatternModulator Clone()
        {
            return new Int32DifferentialFunctionPatternModulator(this._carrier.Clone(), this.Pattern, this.Period);
        }

        public int Reset()
        {
            return this.CurrentPattern(0);
        }

        private int NegativeMove(int dx)
        {
            var x1 = this._x + dx;
            if (x1 >= 0)
                return this.CurrentPattern(x1);

            // compute new origin in current referential
            var o1x = MathEx.Floor(x1, this.Period);
            var o1y = this.Carrier(o1x);

            // exit current cell
            var dy = (this._patternHeight * this.Pattern(0)) - this._y;

            // cross intermediate cells
            var v1y = this.Carrier(o1x + this.Period);
            dy += v1y;
            this._patternHeight = v1y - o1y;

            // move origin to new referential
            this._carrier.Translate(o1x, o1y);
            this._x = x1 - o1x;

            // enter new cell
            return dy += this.EnterPattern(this.Period);
        }

        private int PositiveMove(int dx)
        {
            var x1 = this._x + dx;
            if (x1 <= this.Period)
                return this.CurrentPattern(x1);

            // compute new origin in current referential
            var o1x = MathEx.Floor(x1, this.Period);
            var o1y = this.Carrier(o1x);

            // exit current cell
            var dy = (this._patternHeight * this.Pattern(this.Period)) - this._y;

            // cross intermediate cells
            dy += o1y - this._patternHeight;

            // move origin to new referential
            this._carrier.Translate(o1x, o1y);
            this._x = x1 - o1x;
            this._patternHeight = this.Carrier(this.Period);

            // enter new pattern
            return dy += this.EnterPattern(0);
        }

        private int EnterPattern(int xEntryPoint)
        {
            var y = this._patternHeight * this.Pattern(xEntryPoint);
            this._y = this._patternHeight * this.Pattern(this._x);
            return this._y - y;
        }

        private int CurrentPattern(int x1)
        {
            var y1 = this._patternHeight * this.Pattern(x1);
            var dy = y1 - this._y;
            this._x = x1;
            this._y = y1;
            return dy;
        }
    }
}
