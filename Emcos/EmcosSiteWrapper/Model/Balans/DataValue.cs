namespace TMP.Work.Emcos.Model.Balans
{
    public enum DataValueStatus
    {
        Normal,
        Missing
    }
    public class DataValue : TMP.Work.Emcos.PropertyChangedBase
    {
        private double? _doubleValue = null;
        public double? DoubleValue
        {
            get { return _doubleValue; }
            set { _doubleValue = value; RaisePropertyChanged("DoubleValue"); }
        }
        private double? _percentValue = null;
        public double? PercentValue
        {
            get { return _percentValue; }
            set { _percentValue = value; RaisePropertyChanged("PercentValue"); }
        }
        private DataValueStatus _status = DataValueStatus.Normal;
        public DataValueStatus Status
        {
            get { return _status; }
            set { _status = value; RaisePropertyChanged("Status"); }
        }
    }
}
