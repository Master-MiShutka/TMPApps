using System.Collections.Generic;

namespace TMP.Work.Emcos.Model.Balans
{
    public interface IBalansGroup : IBalansItem
    {
        double? VvodaIn { get; }
        double? VvodaOut { get; }

        double? Tsn { get; }

        double? FideraIn { get; }
        double? FideraOut { get; }

        double? Unbalance { get; }
        double? PercentageOfUnbalance { get; }

        ICollection<IBalansItem> Children { get; set; }
        IList<IBalansItem> Items { get; }
        void UpdateChildren();
    }
}
