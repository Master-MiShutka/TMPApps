using System;
using System.Runtime.InteropServices;

namespace WpfApp_ToastTest.ShellHelpers
{
    internal class PropVariantHelper
    {
        private static class NativeMethods
        {
            [DllImport("Ole32.dll", PreserveSig = false)]
            internal static extern void PropVariantClear(ref ShellHelpers.PROPVARIANT pvar);
        }

        private ShellHelpers.PROPVARIANT variant;
        public ShellHelpers.PROPVARIANT Propvariant
        {
            get { return variant; }
        }

        public VarEnum VarType
        {
            get { return (VarEnum)variant.vt; }
            set { variant.vt = (ushort)value; }
        }

        public void SetValue(Guid value)
        {
            NativeMethods.PropVariantClear(ref variant);
            byte[] guid = ((Guid)value).ToByteArray();
            variant.vt = (ushort)VarEnum.VT_CLSID;
            variant.unionmember = Marshal.AllocCoTaskMem(guid.Length);
            Marshal.Copy(guid, 0, variant.unionmember, guid.Length);
        }

        public void SetValue(string val)
        {
            NativeMethods.PropVariantClear(ref variant);
            variant.vt = (ushort)VarEnum.VT_LPWSTR;
            variant.unionmember = Marshal.StringToCoTaskMemUni(val);
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct PROPERTYKEY
    {
        public Guid fmtid;
        public uint pid;

        public PROPERTYKEY(Guid guid, uint id)
        {
            fmtid = guid;
            pid = id;
        }

        /// <summary>PKEY_AppUserModel_ID</summary>
        public static readonly PROPERTYKEY AppUserModel_ID = new PROPERTYKEY(new Guid("9F4C2855-9F79-4B39-A8D0-E1D42DE1D5F3"), 5);

        /// <summary>PKEY_AppUserModel_ID</summary>
        public static readonly PROPERTYKEY AppUserModel_ToastActivatorCLSID = new PROPERTYKEY(new Guid("9F4C2855-9F79-4B39-A8D0-E1D42DE1D5F3"), 26);
    }
}
