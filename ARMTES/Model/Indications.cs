using System;
using System.Collections.Generic;

namespace TMP.ARMTES.Model
{
    public class Indications
    {
        private double? _tarif0;
        private double? _tarif1;
        private double? _tarif2;
        private double? _tarif3;
        private double? _tarif4;

        public double? Tarriff0 { get { return GetRoundIndication(_tarif0); } set { _tarif0 = value; } }
        public double? Tarriff1 { get { return GetRoundIndication(_tarif1); } set { _tarif1 = value; } }
        public double? Tarriff2 { get { return GetRoundIndication(_tarif2); } set { _tarif2 = value; } }
        public double? Tarriff3 { get { return GetRoundIndication(_tarif3); } set { _tarif3 = value; } }
        public double? Tarriff4 { get { return GetRoundIndication(_tarif4); } set { _tarif4 = value; } }
        public string DataReliability { get; set; }

        public bool HasMissingData
        {
            get
            {
                return String.IsNullOrEmpty(DataReliability) == true || DataReliability != "+";
            }
        }

        public double? this[int index] 
        { 
            get
            {
                return index switch
                {
                    0 => Tarriff0,
                    1 => Tarriff1,
                    2 => Tarriff2,
                    3 => Tarriff3,
                    4 => Tarriff4,
                    _ => throw new ArgumentOutOfRangeException(),
                };
            }
        }

        private double? GetRoundIndication(double? value)
        {
            if (value.HasValue)
                return Math.Floor(value.Value);
            else
                return new Nullable<double>();
        }
    }

    public class IndicationViewItem
    {
        public string AccountingPoint { get; set; }
        public string CounterType { get; set; }
        public Indications PreviousIndications { get; set; }
        public Indications NextIndications { get; set; }
        public double? Difference { get; set; }

    }

    public class QualityIndications
    {
        public string Period { get; set; }
        public List<PointQualityIndications> PointsData { get; set; }
    }
    public class PointQualityIndications
    {
        public string PointName { get; set; }
        public List<PointQualityIndicationsLegend> Values { get; set; }
    }

    public class PointQualityIndicationsLegend
    {
        public string Interval { get; set; }
        public PointQualityIndicationsType Type { get; set; }
        public string Brush
        {
            get
            {
                switch (Type)
                {
                    case PointQualityIndicationsType.Reliable:
                        return "System.Windows.Media.Brushes.Green";
                    case PointQualityIndicationsType.Unreliable:
                        return "System.Windows.Media.Brushes.Red";
                    case PointQualityIndicationsType.NotRead:
                        return "System.Windows.Media.Brushes.White";
                    default:
                        return "System.Windows.Media.Brushes.Black";
                }
            }
        }
    }

    public enum PointQualityIndicationsType
    {
        // Достоверные
        Reliable,
        // Недостоверные
        Unreliable,
        // Дыра, не считаны
        NotRead
    }
}
