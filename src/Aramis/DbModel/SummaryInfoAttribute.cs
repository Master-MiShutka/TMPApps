namespace TMP.WORK.AramisChetchiki.DbModel
{
    using System;

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class SummaryInfoAttribute : Attribute
    {
        public SummaryInfoAttribute()
        {
        }
    }
}
