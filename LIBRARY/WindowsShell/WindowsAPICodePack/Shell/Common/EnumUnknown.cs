// Copyright (c) Microsoft Corporation.  All rights reserved.

namespace Microsoft.WindowsAPICodePack.Shell
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using MS.WindowsAPICodePack.Internal;

    internal class EnumUnknownClass : IEnumUnknown
    {
        private List<ICondition> conditionList = new List<ICondition>();
        private int current = -1;

        internal EnumUnknownClass(ICondition[] conditions)
        {
            this.conditionList.AddRange(conditions);
        }

        #region IEnumUnknown Members

        public HResult Next(uint requestedNumber, ref IntPtr buffer, ref uint fetchedNumber)
        {
            this.current++;

            if (this.current < this.conditionList.Count)
            {
                buffer = Marshal.GetIUnknownForObject(this.conditionList[this.current]);
                fetchedNumber = 1;
                return HResult.Ok;
            }

            return HResult.False;
        }

        public HResult Skip(uint number)
        {
            int temp = this.current + (int)number;

            if (temp > (this.conditionList.Count - 1))
            {
                return HResult.False;
            }

            this.current = temp;
            return HResult.Ok;
        }

        public HResult Reset()
        {
            this.current = -1;
            return HResult.Ok;
        }

        public HResult Clone(out IEnumUnknown result)
        {
            result = new EnumUnknownClass(this.conditionList.ToArray());
            return HResult.Ok;
        }

        #endregion
    }
}