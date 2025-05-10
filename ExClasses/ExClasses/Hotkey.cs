using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using System.ComponentModel;

namespace ExClasses
{
    using static NativeMethod;
    public static class Hotkey
    {
        public static event KeyEventHandler HotkeyPressed;
        static Hotkey()
        {
            Application.AddMessageFilter(new HotkeyMessageFilter());
        }
        public static void RegisterHotkey(Keys key)
        {
            if (key == Keys.None)
            {
                throw new ArgumentNullException(nameof(key) + " can not be 0");
            }
            try
            {
                UnregisterHotKey(IntPtr.Zero, (int)key);
            }
            catch { }
            if (!RegisterHotKey(IntPtr.Zero, (int)key, (int)(key & Keys.Modifiers) >> 16, (int)(key & Keys.KeyCode)))
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
        }

        public static void UnregisterHotkey(Keys key)
        {
            bool res = UnregisterHotKey(IntPtr.Zero, (int)key);
            if (!res)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
        }
        class HotkeyMessageFilter : IMessageFilter
        {
            private const int WM_HOTKEY = 786;

            bool IMessageFilter.PreFilterMessage(ref Message m)
            {
                switch (m.Msg)
                {
                    case WM_HOTKEY:
                        int hId = m.WParam.ToInt32();
                        if (!(HotkeyPressed is null))
                        {
                            int lParam = m.LParam.ToInt32();
                            int vk = (byte)(lParam >> 16);
                            int mks = (byte)(lParam & 0Xffff);
                            HotkeyPressed?.Invoke(this, new KeyEventArgs((Keys)(vk + 0x10000 * mks)));
                        }
                        return true;
                }
                return false;
            }
        }
    }
    internal static partial class NativeMethod
    {
        [DllImport("user32.dll", EntryPoint = "RegisterHotKey")]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);

        [DllImport("user32.dll", EntryPoint = "UnregisterHotKey")]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);
    }

}
