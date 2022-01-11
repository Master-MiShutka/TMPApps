namespace TMP.ExcelXml
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    #region Parameter class

    /// <summary>
    /// Parameter denotes a single parameter in a formula
    /// </summary>
    public class Parameter
    {
        /// <summary>
        /// Parameter type
        /// </summary>
        public ParameterType ParameterType { get; private set; }

        /// <summary>
        /// Value of the parameter
        /// </summary>
        public object Value { get; private set; }

        internal Parameter(char p)
        {
            this.ParameterType = ParameterType.String;
            this.Value = p;
        }

        internal Parameter(string p)
        {
            this.ParameterType = ParameterType.String;
            this.Value = p;
        }

        internal Parameter(Range p)
        {
            this.ParameterType = ParameterType.Range;
            this.Value = p;
        }

        internal Parameter(Formula p)
        {
            this.ParameterType = ParameterType.Formula;
            this.Value = p;
        }
    }
    #endregion

    /// <summary>
    /// Formula is a formula builder class which can be stored directly in a cell
    /// </summary>
    public class Formula
    {
        #region Private and Internal fields
        internal List<Parameter> parameters;
        #endregion

        #region Public Properties

        /// <summary>
        /// Readonly list of formula paramters
        /// </summary>
        public IList<Parameter> Parameters => this.parameters;

        /// <summary>
        /// Check to force parameters in function
        /// </summary>
        /// <remarks>In case if this flag is set and formula does not contain one or more parameters
        /// then when the formula is assigned to a cell, the cell is left empty.</remarks>
        /// <example><code>
        /// sheet[0, 0].Value = 2;
        /// sheet[1, 0].Value = 12;
        /// sheet[2, 0].Value = 9;
        /// sheet[3, 0].Value = 7;
        ///
        /// Formula formula1 = new Formula("Sum", new Range(sheet[0, 0], sheet[3, 0]),
        ///							delegate (Cell cell) { return cell.GetValue() > 10; });
        ///
        /// formula1.MustHaveParameters = false; // default value
        /// sheet[4, 0].Value = formula; // cell value will be '=SUM()')
        ///
        /// Formula formula2 = new Formula("Sum", new Range(sheet[0, 0], sheet[3, 0]),
        ///							delegate (Cell cell) { return cell.GetValue() > 10; });
        ///
        /// formula2.MustHaveParameters = true;
        /// sheet[5, 0].Value = formula; // cell will be empty
        /// </code></example>
        public bool MustHaveParameters { get; set; }
        #endregion

        #region Constructor

        /// <summary>
        /// Formula constructor
        /// </summary>
        public Formula()
        {
            this.parameters = new List<Parameter>();
        }
        #endregion

        #region Private and Internal methods
        internal string ToString(Cell cell)
        {
            StringBuilder parameterList = new StringBuilder();

            foreach (Parameter p in this.Parameters)
            {
                switch (p.ParameterType)
                {
                    case ParameterType.Range:
                        Range range = p.Value as Range;

                        if (range != null)
                        {
                            parameterList.Append(range.RangeReference(cell));
                        }

                        break;

                    case ParameterType.Formula:
                        Formula formula = p.Value as Formula;

                        if (formula != null)
                        {
                            parameterList.Append(formula.ToString(cell));
                        }

                        break;

                    case ParameterType.String:
                        parameterList.Append(p.Value.ToString());
                        break;
                }
            }

            if (this.MustHaveParameters && (this.Parameters.Count == 0 || parameterList.Length == 0))
            {
                return string.Empty;
            }

            return parameterList.ToString();
        }
        #endregion

        #region Public methods

        /// <summary>
        /// Adds a operator as a parameter in a formula
        /// </summary>
        /// <param name="op">Operator to add as parameter</param>
        public Formula Add(char op)
        {
            Parameter p = new Parameter(op);

            this.Parameters.Add(p);

            return this;
        }

        /// <summary>
        /// Adds a operator as a parameter in a formula
        /// </summary>
        /// <param name="op">Operator to add as parameter</param>
        public Formula Operator(char op)
        {
            Parameter p = new Parameter(op);

            this.Parameters.Add(p);

            return this;
        }

        /// <summary>
        /// Starts a new group
        /// </summary>
        public Formula StartGroup()
        {
            Parameter p = new Parameter('(');

            this.Parameters.Add(p);

            return this;
        }

        /// <summary>
        /// Ends a group
        /// </summary>
        public Formula EndGroup()
        {
            Parameter p = new Parameter(')');

            this.Parameters.Add(p);

            return this;
        }

        /// <summary>
        /// Adds a empty group to the formula
        /// </summary>
        /// <returns></returns>
        public Formula EmptyGroup()
        {
            Parameter p = new Parameter("()");

            this.Parameters.Add(p);

            return this;
        }

        /// <summary>
        /// Adds a cell as a parameter in a formula
        /// </summary>
        /// <param name="cell">Cell to add as parameter</param>
        public Formula Add(Cell cell)
        {
            Parameter p = new Parameter(new Range(cell));

            this.Parameters.Add(p);

            return this;
        }

        /// <summary>
        /// Adds a range as a parameter in a formula
        /// </summary>
        /// <param name="range">Range to add as parameter</param>
        public Formula Add(Range range)
        {
            Parameter p = new Parameter(range);

            this.Parameters.Add(p);

            return this;
        }

        /// <summary>
        /// Adds a string as a parameter in a formula
        /// </summary>
        /// <param name="parameter">String to add as parameter</param>
        public Formula Add(string parameter)
        {
            Parameter p = new Parameter(parameter);

            this.Parameters.Add(p);

            return this;
        }

        /// <summary>
        /// Adds another formula as a parameter in a formula
        /// </summary>
        /// <param name="formula">Another formula to add to this formula's parameter list</param>
        public Formula Add(Formula formula)
        {
            Parameter p = new Parameter(formula);

            this.Parameters.Add(p);

            return this;
        }

        /// <summary>
        /// Adds a filtered range as a parameter
        /// </summary>
        /// <param name="range">Range to add as parameter</param>
        /// <param name="cellCompare">A custom defined cell to compare the values of the range</param>
        public Formula Add(Range range, Predicate<Cell> cellCompare)
        {
            if (range.CellFrom == null)
            {
                return this;
            }

            if (range.CellTo == null)
            {
                if (cellCompare(range.CellFrom))
                {
                    this.Add(range);
                }

                return this;
            }

            Worksheet ws = range.CellFrom.ParentRow.ParentSheet;

            int rowFrom = range.CellFrom.ParentRow.RowIndex;
            int rowTo = range.CellTo.ParentRow.RowIndex;

            int cellIndexFrom = range.CellFrom.CellIndex;
            int cellIndexTo = range.CellTo.CellIndex;

            for (int j = cellIndexFrom; j <= cellIndexTo; j++)
            {
                // Find the first row in column which matches the style
                int rangeRowIndex = rowFrom;

                do
                {
                    if (cellCompare(ws[j, rangeRowIndex]))
                    {
                        for (int i = rangeRowIndex + 1; i <= rowTo; i++)
                        {
                            if (!cellCompare(ws[j, i]))
                            {
                                this.Add(new Range(ws[j, rangeRowIndex], ws[j, i - 1]));

                                rangeRowIndex = i;

                                break;
                            }
                        }
                    }
                    else
                    {
                        rangeRowIndex++;
                    }
                }
                while (rangeRowIndex <= rowTo);
            }

            return this;
        }
        #endregion
    }
}