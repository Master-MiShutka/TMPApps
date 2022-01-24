namespace TMP.WORK.AramisChetchiki.ViewModel;

using System;
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
    }

    public Meter Meter { get; init; }

    public IEnumerable<ChangeOfMeter> MeterChanges { get; init; }

    public IEnumerable<PaymentData> Payments { get; init; }

    public IEnumerable<MeterControlData> MeterControlData { get; init; }
}
