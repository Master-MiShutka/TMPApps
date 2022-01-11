/*************************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license

   For more features, controls, and fast professional support,
   pick up the Plus Edition at http://xceed.com/wpf_toolkit

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/

namespace DataGridWpf.Export
{
    using System;

    public class CsvFormatSettings : FormatSettingsBase
    {
        public CsvFormatSettings()
        {
            this.Separator = ',';
            this.TextQualifier = '"';
            this.NewLine = Environment.NewLine;
        }

        public char Separator
        {
            get;
            set;
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
    }
}
