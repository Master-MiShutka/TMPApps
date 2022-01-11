namespace TMP.Shared
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public interface IWaitableObject : IStateObject, ICancelable
    {
    }
}