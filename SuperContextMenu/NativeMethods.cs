using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Zhwang.SuperContextMenu
{
    internal class NativeMethods
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern bool SetMenuItemInfo(HandleRef hMenu, int uItem, bool fByPosition, MENUITEMINFO_T_RW lpmii);

        [DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr GetForegroundWindow();
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public class MENUITEMINFO_T_RW
    {
        public int cbSize = Marshal.SizeOf(typeof(MENUITEMINFO_T_RW));
        public int fMask = 0x00000080; //MIIM_BITMAP = 0x00000080
        public int fType;
        public int fState;
        public int wID;
        public IntPtr hSubMenu = IntPtr.Zero;
        public IntPtr hbmpChecked = IntPtr.Zero;
        public IntPtr hbmpUnchecked = IntPtr.Zero;
        public IntPtr dwItemData = IntPtr.Zero;
        public IntPtr dwTypeData = IntPtr.Zero;
        public int cch;
        public IntPtr hbmpItem = IntPtr.Zero;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public class MENUINFO
    {
        public int cbSize = Marshal.SizeOf(typeof(MENUINFO));
        public int fMask = 0x00000010; //MIM_STYLE;
        public int dwStyle = 0x04000000; //MNS_CHECKORBMP;
        public uint cyMax;
        public IntPtr hbrBack = IntPtr.Zero;
        public int dwContextHelpID;
        public IntPtr dwMenuData = IntPtr.Zero;
    }
}
