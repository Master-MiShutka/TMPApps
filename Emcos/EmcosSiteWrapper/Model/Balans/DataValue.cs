namespace TMP.Work.Emcos.Model.Balans
{
    public enum ValueStatus
    {
        Normal,
        Missing
    }
    public class Value : TMP.Work.Emcos.PropertyChangedBase
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
        private ValueStatus _status = ValueStatus.Normal;
        public ValueStatus Status
        {
            get { return _status; }
            set { _status = value; RaisePropertyChanged("Status"); }
        }
    }
}
