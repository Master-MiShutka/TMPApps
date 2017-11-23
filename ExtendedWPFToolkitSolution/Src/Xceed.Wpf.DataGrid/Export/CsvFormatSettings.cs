/*************************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at http://xceed.com/wpf_toolkit

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

namespace Xceed.Wpf.DataGrid.Export
{
    public class CsvFormatSettings : IFormatSettings
    {
        public CsvFormatSettings()
        {
            this.SeparatorType = CsvValueSeparatorType.Comma;
            this.TextQualifier = '"';
            this.NewLine = Environment.NewLine;
            this.Culture = CultureInfo.CurrentCulture;

            this.DateTimeFormat = "dd.MM.yyy";
            this.NumericFormat = "G";
            this.FloatingPointFormat = "F3";
        }

        public CsvValueSeparatorType SeparatorType
        {
            get;
            set;
        }
        private char _separator;
        public char Separator
        {
            get
            {
                switch (SeparatorType)
                {
                    case CsvValueSeparatorType.Comma:
                        return ',';
                    case CsvValueSeparatorType.Tab:
                        return '\t';
                    case CsvValueSeparatorType.Semicolon:
                        return ';';
                    case CsvValueSeparatorType.Custom:
                        return _separator;
                    default:
                        throw new NotImplementedException("SeparateType");
                }
            }
            set
            {
                if (SeparatorType != CsvValueSeparatorType.Custom)
                    throw new InvalidOperationException("SeparateType must be CsvValueSeparateType.Custom");
                _separator = value;
            }
        }

        public char TextQualifier
        {
            get;
            set;
        }

        public string NewLine
        {
            get;
            set;
        }
        public string DateTimeFormat { get ; set ; }
        public string NumericFormat { get ; set ; }
        public string FloatingPointFormat { get ; set ; }
        public CultureInfo Culture { get ; set ; }
    }
}
