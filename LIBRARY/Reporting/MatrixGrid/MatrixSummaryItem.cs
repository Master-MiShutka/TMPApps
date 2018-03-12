namespace TMP.UI.Controls.WPF.Reporting.MatrixGrid
{
    public class MatrixSummaryItem : MatrixItemBase
    {
        public MatrixSummaryItem(int value)
        {
            this.Value = value;
        }

        public int Value { get; private set; }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}
