﻿using System;

namespace TMP.Wpf.Common.Controls.TableView
{
  public class TableViewColumnEventArgs : EventArgs
  {
    public TableViewColumnEventArgs(TableViewColumn column)
    {
      Column = column;
    }
    public TableViewColumn Column { get; private set; }
  }
}
