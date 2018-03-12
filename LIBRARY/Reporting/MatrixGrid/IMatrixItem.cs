namespace TMP.UI.Controls.WPF.Reporting.MatrixGrid
{
    /// <summary>
    /// Интерфейс для ячеек матрицы
    /// </summary>
    public interface IMatrixItem
    {
        int GridRow { get;  }
        int GridRowSpan { get;  }
        int GridColumn { get;  }
        int GridColumnSpan { get;  }

        string ToolTip { get;  }
    }
}
