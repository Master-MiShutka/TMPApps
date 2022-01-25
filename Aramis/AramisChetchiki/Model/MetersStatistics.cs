namespace TMP.WORK.AramisChetchiki.Model
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Input;
    using TMP.Shared.Commands;
    using TMP.WORK.AramisChetchiki.ViewModel;

    public class MetersStatistics
    {
        public MetersStatistics(IEnumerable<Meter> metersList)
        {
            this.MetersCount = metersList.Count();

            int mainViewModelMetersCount = MainViewModel.Meters.Count();

            var listOfПоверенMeters = MainViewModel.Meters.Where(m => m.Поверен == false).ToList();
            var listOfЭлектронныйMeters = MainViewModel.Meters.Where(m => m.Электронный).ToList();
            this.Message = string.Format(AppSettings.CurrentCulture, "Всего счётчиков: {0:N0}, с истёкшим сроком метрологической поверки: {1:N0} или {2:N1}%. Доля электронных счётчиков: {3:N1}%.",
                mainViewModelMetersCount,
                listOfПоверенMeters.Count,
                100d * listOfПоверенMeters.Count / mainViewModelMetersCount,
                100d * listOfЭлектронныйMeters.Count / mainViewModelMetersCount);

            var diffTariffMeters = metersList
                .GroupBy(
                    d => (d.НомерСчетчика, d.БазовыйЛицевой),
                    (i, meters) => new
                    {
                        Group = i,
                        Meters = meters,
                        MetersCount = meters.Count(),
                    })
                    .Where(i => i.MetersCount > 1)
#if DEBUG
                    .ToList();
#else
;
#endif

            var diffTariff2Meters = diffTariffMeters
                .Where(i => i.MetersCount == 2)
                .Distinct()
                .SelectMany(i => i.Meters);

            this.DiffTariff2MetersCount = diffTariff2Meters.Count() / 2;

            var diffTariff3Meters = diffTariffMeters
                .Where(i => i.MetersCount == 3)
                .Distinct()
                .SelectMany(i => i.Meters);

            this.DiffTariff3MetersCount = diffTariff3Meters.Count() / 3;

            this.PersonalAccountsCount = metersList.Select(d => d.БазовыйЛицевой).Distinct().Count();

            var metersNotSplit = metersList
                    .Where(i => i.МестоУстановки != "СПЛИТ")
#if DEBUG
                    .ToList();
#else
;
#endif
            this.VisitedLastYearCount = metersNotSplit.Where(i => i.ОбходаНеБылоМесяцев < 12).Count();

            var notVisitedLastYear = metersNotSplit.Where(i => i.ОбходаНеБылоМесяцев >= 12 && i.ОбходаНеБылоМесяцев < 24);
            this.NotVisitedLastYearCount = notVisitedLastYear.Count();
            this.CommandShowNotVisitedLastYear = new DelegateCommand(() => MainViewModel.ShowMetersCollection(notVisitedLastYear));

            var notVisitedFromTwoToThreeYears = metersNotSplit.Where(i => i.ОбходаНеБылоМесяцев >= 24 && i.ОбходаНеБылоМесяцев < 36);
            this.NotVisitedFromTwoToThreeYearsCount = notVisitedFromTwoToThreeYears.Count();
            this.CommandShowNotVisitedFromTwoToThreeYears = new DelegateCommand(() => MainViewModel.ShowMetersCollection(notVisitedFromTwoToThreeYears));

            var notVisitedMoreThreeYears = metersNotSplit.Where(i => i.ОбходаНеБылоМесяцев >= 36);
            this.NotVisitedMoreThreeYearsCount = notVisitedMoreThreeYears.Count();
            this.CommandShowNotVisitedMoreThreeYears = new DelegateCommand(() => MainViewModel.ShowMetersCollection(notVisitedMoreThreeYears));

            var m = metersList.GroupBy(d => (d.БазовыйЛицевой, d.НомерСчетчика)).Distinct().Count();

            var y = metersList == null ? 0 : metersList.GroupBy(meter => meter.БазовыйЛицевой, n => n.НомерСчетчика, (acc, n) => n.Distinct().Count()).Sum();
            var q = metersList == null ? 0 : metersList.Select(d => d.Лицевой).Count();

            var t = metersList
                .GroupBy(meter => meter.БазовыйЛицевой, n => n.НомерСчетчика, (acc, n) => n.Distinct().ToArray())
                .ToArray();

            var n = metersList.Select(d => d.НомерСчетчика).Distinct().Count();

            this.CommandShowAll = new DelegateCommand(() => MainViewModel.ChangeMode(Mode.ViewCollection));

            this.CommandShowDiffTariff2Meters = new DelegateCommand(() => MainViewModel.ShowMetersCollection(diffTariff2Meters));

            this.CommandShowDiffTariff3Meters = new DelegateCommand(() => MainViewModel.ShowMetersCollection(diffTariff3Meters));
        }

        internal static IMainViewModel MainViewModel => TMPApplication.TMPApp.Instance?.MainViewModel as IMainViewModel;

        /// <summary>
        /// Текст сообщения
        /// </summary>
        public string Message { get; init; }

        public int MetersCount { get; init; }

        public int DiffTariff2MetersCount { get; init; }

        public int DiffTariff3MetersCount { get; init; }

        public int PersonalAccountsCount { get; init; }

        public int VisitedLastYearCount { get; init; }

        public int NotVisitedLastYearCount { get; init; }

        public int NotVisitedFromTwoToThreeYearsCount { get; init; }

        public int NotVisitedMoreThreeYearsCount { get; init; }

        public ICommand CommandShowAll { get; }

        public ICommand CommandShowDiffTariff2Meters { get; }

        public ICommand CommandShowDiffTariff3Meters { get; }

        public ICommand CommandShowNotVisitedLastYear { get; }

        public ICommand CommandShowNotVisitedFromTwoToThreeYears { get; }

        public ICommand CommandShowNotVisitedMoreThreeYears { get; }

        //public ICommand CommandShowAll { get; }
    }
}
