namespace TMP.WORK.AramisChetchiki.ViewModel;
using System.Collections.Generic;
using System.Linq;
using TMP.WORK.AramisChetchiki.Model;

public class MeterViewViewModel : BaseDataViewModel<Meter>
{
    public MeterViewViewModel(Meter meter)
    {
        this.Meter = meter;

        this.MeterChanges = MainViewModel.Data.ChangesOfMeters.ContainsKey(meter.Лицевой)
            ? MainViewModel.Data.ChangesOfMeters[meter.Лицевой]
            : new List<ChangeOfMeter>();

        this.Payments = MainViewModel.Data.Payments.ContainsKey(meter.Лицевой)
            ? MainViewModel.Data.Payments[meter.Лицевой].SelectMany(p => p.Payments)
            : new List<PaymentData>();

        this.MeterControlData = MainViewModel.Data.MetersControlData.ContainsKey(meter.Лицевой)
            ? MainViewModel.Data.MetersControlData[meter.Лицевой].FirstOrDefault().Data
            : new List<MeterControlData>();

        this.Events = MainViewModel.Data.Events.ContainsKey(meter.Лицевой)
            ? new MeterEventsCollection(MainViewModel.Data.Events[meter.Лицевой].OrderBy(i => i.Date))
            : new MeterEventsCollection();

        this.RaisePropertyChanged(nameof(this.Events));
    }

    public Meter Meter { get; init; }

    public IEnumerable<ChangeOfMeter> MeterChanges { get; init; }

    public IEnumerable<PaymentData> Payments { get; init; }

    public IEnumerable<MeterControlData> MeterControlData { get; init; }

    public MeterEventsCollection Events { get; init; }

    public override int GetHashCode()
    {
        System.Guid guid = new System.Guid("1A555AD8-D371-4E35-9852-0967B8EC0458");
        return guid.GetHashCode();
    }
}
