using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TMP.ARMTES.Model
{
    //////

    public class SelectElementModel
    {
        public List<Collector> Collectors { get; set; }

        public Statistics Statistics { get; set; }
        public SelectElementModel()
        {
            Collectors = new List<Collector>();
            Statistics = new Statistics();
        }
    }

    public class ViewDeviceModel
    {      
        public SessionInformation Session { get; set; }

        public List<IndicationViewItem> CountersIndications { get; set; }

        public List<QualityIndications> QualityIndications { get; set; }

        public ViewDeviceModel()
        {
            Session = new SessionInformation();
            CountersIndications = new List<IndicationViewItem>();
            QualityIndications = new List<ARMTES.Model.QualityIndications>();
        }
    }

    public class ViewCounterModel
    {
        public string AccountPoint { get; set; }
        public string CounterType { get; set; }
        public string CounterNumber { get; set; }
        public string CounterNetworkAddress { get; set; }
        public string Ktt { get; set; }
        public string CounterManufacturer { get; set; }
        public string DogNumber { get; set; }
        public string AccountType { get; set; }

        public string AbonentFullName { get; set; }
        public string AbonentName { get; set; }
        public string AbonentShortName { get; set; }
        public string AbonentAddress { get; set; }

        public string ObjectName { get; set; }
        public string AccountPointName { get; set; }
        public string Substation { get; set; }
        public string Fider { get; set; }
        public string TP { get; set; }
        public string ObjectAddress { get; set; }
        public string ObjectState { get; set; }


        public string Departament { get; set; }
        public string AmperCounterNumber { get; set; }
        public string AmperParentPointId { get; set; }
        public string AmperPointId { get; set; }
        public string Status { get; set; }
        public DateTime LastSessionDate { get; set; }

        public IndicationViewItem IndicationViewItem { get; set; }
        public ViewCounterModel()
        {
            IndicationViewItem = new IndicationViewItem();
        }
    }

    public class ArmtesElement
    {
        public ArmtesElement()
        {
            Items = new List<ArmtesElement>();
        }

        public string Label { get; set; }
        public string ParentId { get; set; }
        public string Value { get; set; }
        public List<ArmtesElement> Items { get; set; }
        public ArmtesElement Parent { get; set; }
    }

    public class PostTariffIndications
    {
        public int ParameterId { get; set; }
        public string ParameterName { get; set; }
        public byte ParametersCount { get; set; }
        public IndicationViewItem IndicationViewItem { get; set; }
        public ChartStatisticsViewModel ChartStatisticsViewModel { get; set; }
        public object SecondChartStatisticsViewModel { get; set; }
        public object ReferenceChartStatisticsViewModel { get; set; }
        public CounterChoosingViewModel CounterChoosingViewModel { get; set; }
        public string[] Dates { get; set; }
    }

    public class ChartStatisticsViewModel
    {
        public string MinDate { get; set; }
        public string MaxDate { get; set; }
        public string ChartTitle { get; set; }
        public string[] Flats { get; set; }
        public ChartStatisticsViewPoint[] ChartStatisticsViewPoints { get; set; }

    }

    public class ChartStatisticsViewPoint
    {
        public string TimeStamp { get; set; }
        public double BalanceValue { get; set; }
    }

    public class CounterChoosingViewModel
    {
        public string[] CounterChoosingViewItems { get; set; }
    }

    public class SessionInformation
    {
        /// <summary>
        /// Производитель модема
        /// </summary>
        public string ModemManufacturer { get; set; }
        /// <summary>
        /// Модель устройства
        /// </summary>
        public string Model { get; set; }
        /// <summary>
        /// Описание
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// Текущий статус
        /// </summary>
        public string CurrentStatus { get; set; }
        /// <summary>
        /// Последний сеанс
        /// </summary>
        public DateTime LastSessionDate { get; set; }
    }
}