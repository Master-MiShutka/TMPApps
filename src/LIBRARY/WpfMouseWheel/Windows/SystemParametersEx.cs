namespace WpfMouseWheel.Windows
{
    using System.ComponentModel;
    using System.Runtime.InteropServices;
    using System.Security;
    using Microsoft.Win32;

    public static class SystemParametersEx
    {
        public static int WheelScrollChars
        {
            get
            {
                if (SystemParametersEx.wheelScrollChars < 0)
                {
                    lock (SyncMouse)
                    {
                        if (SystemParametersEx.wheelScrollChars < 0)
                        {
                            if (!SystemParametersInfo(0x006C, 0, ref SystemParametersEx.wheelScrollChars, 0))
                                throw new Win32Exception();
                        }
                    }
                }

                return SystemParametersEx.wheelScrollChars;
            }
        }

        internal static void Initialize()
        {
        }

        static SystemParametersEx()
        {
            SystemEvents.UserPreferenceChanged += OnUserPreferenceChanged;
        }

        private static object SyncMouse => typeof(SystemParametersEx);

        private static void OnUserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
        {
            if (e.Category == UserPreferenceCategory.Mouse)
            {
                lock (SyncMouse)
                {
                    SystemParametersEx.wheelScrollChars = -1;
                }
            }
        }

        [SuppressUnmanagedCodeSecurity, SecurityCritical, DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool SystemParametersInfo(int nAction, int nParam, ref int value, int ignore);

        private static int wheelScrollChars = -1;
    }
}
