using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TMP.Common.Logger
{
    public interface IAppWithLogger
    {
        ILoggerFacade Log { get; }
    }
}
