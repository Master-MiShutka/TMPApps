using System;
using System.Windows;
using System.Diagnostics;
using System.Windows.Interop;
using System.Runtime.InteropServices;

namespace TMP.WORK.AramisChetchiki
{
    public class NoSystemMenuException : System.Exception
    {
    }
    public static class SystemMenu
    {
        public enum MenuItemFlags : uint
        {
            MF_UNCHECKED = 0x00000000,
            MF_ENABLED = 0x00000000,
            MF_STRING = 0x00000000,
            MF_DISABLED = 0x00000002,
            MF_GRAYED = 0x00000001,
            MF_CHECKED = 0x00000008,
            NF_POPUP = 0x00000010,
            MF_SEPARATOR = 0x00000800
        }

        public enum MenuInfoFlags : uint
        {
            MIIM_STATE = 0x1,
            MIIM_ID = 0x2,
            MIIM_SUBMENU = 0x4,
            MIIM_TYPE = 0x10,
            MIIM_DATA = 0x20,
            MIIM_STRING = 0x40
        }

        [Flags] 
        public enum MenuFlags : uint 
        { 
            MF_STRING = 0, 
            MF_BYPOSITION = 0x400, 
            MF_SEPARATOR = 0x800, 
            MF_REMOVE = 0x1000, 
        } 

        // Contains information about a menu item
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct MENUITEMINFO
        {
            public uint cbSize;
            public MenuInfoFlags fMask;
            public MenuItemFlags fType;
            public MenuItemFlags fState;
            public uint wID;
            public IntPtr hSubMenu;
            public IntPtr hbmpChecked;
            public IntPtr hbmpUnchecked;
            public IntPtr dwItemData;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string dwTypeData;
            public uint cch;
            public IntPtr hbmpItem;
            // Return the size of the structure 
            public static uint sizeOf 
            { 
                get { return (uint)Marshal.SizeOf(typeof(MENUITEMINFO)); } 
            }
        }

        public enum WindowMessages
        {
            wmSysCommand = 0x0112
        }
        
        #region Declarations

        [DllImport("user32.dll", EntryPoint = "DrawMenuBarW", CharSet = CharSet.Unicode, SetLastError = true, ExactSpelling = true,
           CallingConvention = CallingConvention.Winapi)] 
        static extern bool apiDrawMenuBar(IntPtr hWnd);

        [DllImport("user32.dll", EntryPoint = "GetSystemMenu", CharSet = CharSet.Unicode, SetLastError = true, ExactSpelling = true,
           CallingConvention = CallingConvention.Winapi)] 
        static extern IntPtr apiGetSystemMenu(IntPtr hWnd, bool bRevert);

        [DllImport("user32.dll", EntryPoint = "AppendMenuW", CharSet = CharSet.Unicode, SetLastError = true, ExactSpelling = true,
           CallingConvention = CallingConvention.Winapi)] 
        static extern bool apiAppendMenu(IntPtr hMenu, MenuFlags uFlags, uint uIDNewItem, string lpNewItem);

        [DllImport("user32.dll", EntryPoint = "InsertMenuItemW", CharSet = CharSet.Unicode, SetLastError = true, ExactSpelling = true,
           CallingConvention = CallingConvention.Winapi)] 
        static extern bool apiInsertMenuItem(IntPtr hMenu, uint uItem, bool fByPosition,[In] ref MENUITEMINFO lpmii);

        [DllImport("user32.dll", EntryPoint = "GetMenuItemCountW", CharSet = CharSet.Unicode, SetLastError = true, ExactSpelling = true,
           CallingConvention = CallingConvention.Winapi)] 
        static extern int apiGetMenuItemCount(IntPtr hMenu);

        [DllImport("user32.dll", EntryPoint = "GetMenuItemInfoW", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true,
           CallingConvention = CallingConvention.Winapi)] 
        static extern bool apiGetMenuItemInfo(IntPtr hMenu, uint uItem, bool fByPosition, ref MENUITEMINFO lpmii);

        [DllImport("USER32", EntryPoint = "CreatePopupMenu", SetLastError = true, CharSet = CharSet.Unicode, ExactSpelling = true,
           CallingConvention = CallingConvention.Winapi)]
        static extern IntPtr apiCreatePopupMenu();

        /* // And we need the AppendMenu() function. Since .NET uses Unicode,
        // we pick the unicode solution.
        [DllImport("USER32", EntryPoint = "AppendMenuW", SetLastError = true,
                   CharSet = CharSet.Unicode, ExactSpelling = true,
                   CallingConvention = CallingConvention.Winapi)]
        private static extern int apiAppendMenu(IntPtr MenuHandle, int Flags, int NewID, String Item);

        // And we also may need the InsertMenu() function.
        [DllImport("USER32", EntryPoint = "InsertMenuW", SetLastError = true,
                   CharSet = CharSet.Unicode, ExactSpelling = true,
                   CallingConvention = CallingConvention.Winapi)]
        private static extern int apiInsertMenu(IntPtr hMenu, Int32 Position, int Flags, int NewId, String Item);

        [DllImport("USER32", EntryPoint = "InsertMenuItemW", SetLastError = true,
                   CharSet = CharSet.Unicode, ExactSpelling = true,
                   CallingConvention = CallingConvention.Winapi)]
        private static extern int apiInsertMenuItem(IntPtr hMenu, int uItem, int fByPosition, ref MENUITEMINFO lpmii);

        [DllImport("USER32", EntryPoint = "GetMenuItemInfoW", SetLastError = true,
                   CharSet = CharSet.Unicode, ExactSpelling = true,
                   CallingConvention = CallingConvention.Winapi)]
        private static extern int apiGetMenuItemInfo(IntPtr hMenu, int uItem, int fByPosition, ref MENUITEMINFO lpmii); */

        #endregion

        #region Create a new submenu structure
        public static IntPtr AddSysMenuSubMenu()
        {
            //Creates a menu structure and returns
            IntPtr hMenu = apiCreatePopupMenu();
            return hMenu;
        }
        #endregion

        #region Add menu item/submenu structure to system menu
        public static void AddSysMenuItem(String Text, uint ID, uint Position, IntPtr hMenu)
        {
            MENUITEMINFO mii = new MENUITEMINFO();
            IntPtr m_SysMenu = IntPtr.Zero;
            //Check system menu handle
            if (m_SysMenu == IntPtr.Zero)
                m_SysMenu = apiGetSystemMenu(Handle, false);
            
            //Create new menu item info

            if (hMenu != IntPtr.Zero)
            {
                mii.fMask = MenuInfoFlags.MIIM_ID| MenuInfoFlags.MIIM_TYPE | MenuInfoFlags.MIIM_STATE | MenuInfoFlags.MIIM_SUBMENU;
                mii.hSubMenu = hMenu;
            }
            else
                mii.fMask = MenuInfoFlags.MIIM_ID | MenuInfoFlags.MIIM_TYPE | MenuInfoFlags.MIIM_STATE;

            mii.wID = ID;
            if (Text == "-")
                mii.fType = MenuItemFlags.MF_SEPARATOR;
            else
                mii.fType = MenuItemFlags.MF_STRING;

            mii.dwTypeData = Text;
            mii.cch = (uint)mii.dwTypeData.Length;
            mii.fState = MenuItemFlags.MF_ENABLED;
            mii.cbSize = MENUITEMINFO.sizeOf;

            //Insert it
            InsertMenu(m_SysMenu, Position, mii);
        }
        #endregion

        #region Add items to submenu structure
        public static void AddSysMenuSubItem(String Text, uint Position, uint ID, IntPtr hMenu)
        {
            MENUITEMINFO mii = new MENUITEMINFO();
                
            mii.fMask = MenuInfoFlags.MIIM_ID | MenuInfoFlags.MIIM_TYPE | MenuInfoFlags.MIIM_STATE;
            mii.wID = ID;
            mii.hSubMenu = hMenu;
            mii.fType = MenuItemFlags.MF_STRING;
            mii.dwTypeData = Text;
            mii.cch = (uint)mii.dwTypeData.Length;
            mii.fState = MenuItemFlags.MF_ENABLED;
            mii.cbSize = MENUITEMINFO.sizeOf;

            InsertMenu(hMenu, Position, mii);
        }
        #endregion

        #region Insert menu item
        private static void InsertMenu(IntPtr hMenu, uint Position, MENUITEMINFO MenuInfo)
        {
            apiInsertMenuItem(hMenu, Position, true, ref MenuInfo);
        }
        #endregion

        public static IntPtr Handle
        {
            get { return new WindowInteropHelper(Application.Current.MainWindow).Handle; }
        }
    }
}
