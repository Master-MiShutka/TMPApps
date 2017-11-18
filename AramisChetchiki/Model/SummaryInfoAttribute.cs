using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace TMP.WORK.AramisChetchiki.Model
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class SummaryInfoAttribute : Attribute
    {
        public SummaryInfoAttribute() { }
    }
}
