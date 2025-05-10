using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.Diagnostics;
using System.ComponentModel;

namespace ExClasses
{
    using static NativeMethod;
    public class Hook:IDisposable
    {
        private const int WH_KEYBOARD_LL = 13;
        private const int WH_MOUSE_LL = 14;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_KEYUP = 0x0101;
        private static int keyhookId = 0;
        private static int mousehookId = 0;

        private const int WM_CAPTURECHANGED = 0x0215;
        private const int WM_MBUTTONDBLCLK = 0x0209;
        private const int WM_MBUTTONDOWN = 0x0207;
        private const int WM_MBUTTONUP = 0x0208;
        private const int WM_MOUSEHOVER = 0x02A1;
        private const int WM_MOUSEHWHEEL = 0x020E;
        private const int WM_MOUSELEAVE = 0x02A3;
        private const int WM_MOUSEMOVE = 0x0200;
        private const int WM_MOUSEWHEEL = 0x020A;
        private const int WM_XBUTTONDBLCLK = 0x020D;
        private const int WM_XBUTTONDOWN = 0x020B;
        private const int WM_XBUTTONUP = 0x020C;
        private const int WM_LBUTTONDOWN = 0x0201;
        private const int WM_LBUTTONDBLCLK = 0x0203;
        private const int WM_LBUTTONUP = 0x0202;
        private const int WM_RBUTTONDBLCLK = 0x0206;
        private const int WM_RBUTTONDOWN = 0x0204;
        private const int WM_RBUTTONUP = 0x0205;
        internal delegate int HookProc(int nCode, int wParam, int lParam);
        public event KeyEventHandler KeyDown;
        public event KeyEventHandler KeyUp;
        public event MouseEventHandler MouseDown;
        public event MouseEventHandler MouseUp;
        public event MouseEventHandler MouseDoubleClick;
        public event MouseEventHandler MouseHover;
        public event MouseEventHandler MouseWheel;
        public event MouseEventHandler MouseHWheel;
        public event MouseEventHandler MouseMove;
        public event MouseEventHandler MouseLeave;
        public event CaptureChangeHandler CaptureChange;
        public delegate void CaptureChangeHandler(IntPtr ptr);
        public void Main()
        {
            keyhookId = SetHook(WH_KEYBOARD_LL, KeyHookCallback);
            mousehookId = SetHook(WH_MOUSE_LL, MouseHookCallback);
        }
        private bool disposed = false;
        public void Dispose()
        {
            if (!UnhookWindowsHookEx(keyhookId)|| 
                !UnhookWindowsHookEx(mousehookId))
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
            disposed = true;
        }
        ~Hook(){
            if (!disposed)
            {
                Dispose();
            }
        }
        private static int moduleHandle = GetModuleHandle(Process.GetCurrentProcess().MainModule.ModuleName);
        private static int SetHook(int hooktype, HookProc hookProc)
        {
            int res= SetWindowsHookEx(hooktype, hookProc, moduleHandle, 0);
            if (res == 0)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
            return res;
            
        }

        private int KeyHookCallback(int nCode, int wParam, int lParam)
        {
            if (nCode >= 0)
            {
                if ((int)wParam == WM_KEYDOWN)
                {
                    KeyDown?.Invoke(null, new KeyEventArgs((Keys)wParam));
                }
                else if (wParam == (int)WM_KEYUP)
                {
                    KeyUp?.Invoke(null, new KeyEventArgs((Keys)wParam));
                }
            }
            return CallNextHookEx(keyhookId, nCode, wParam, lParam);
        }
        private int MouseHookCallback(int nCode, int wParam, int lParam)
        {
            if (nCode >= 0)
            {
                int x = GetHigh((IntPtr)lParam), y = GetLow((IntPtr)lParam);
                int tmpnum;
                switch ((int)wParam)
                {
                    case WM_LBUTTONDOWN:
                        MouseDown?.Invoke(null, new MouseEventArgs(MouseButtons.Left, 0, x, y, 0));
                        break;
                    case WM_LBUTTONUP:
                        MouseUp?.Invoke(null, new MouseEventArgs(MouseButtons.Left, 0, x, y, 0));
                        break;
                    case WM_LBUTTONDBLCLK:
                        MouseDoubleClick?.Invoke(null, new MouseEventArgs(MouseButtons.Left, 0, x, y, 0));
                        break;
                    case WM_RBUTTONDOWN:
                        MouseDown?.Invoke(null, new MouseEventArgs(MouseButtons.Right, 0, x, y, 0));
                        break;
                    case WM_RBUTTONUP:
                        MouseUp?.Invoke(null, new MouseEventArgs(MouseButtons.Right, 0, x, y, 0));
                        break;
                    case WM_RBUTTONDBLCLK:
                        MouseDoubleClick?.Invoke(null, new MouseEventArgs(MouseButtons.Right, 0, x, y, 0));
                        break;
                    case WM_MBUTTONDOWN:
                        MouseDown?.Invoke(null, new MouseEventArgs(MouseButtons.Middle, 0, x, y, 0));
                        break;
                    case WM_MBUTTONUP:
                        MouseUp?.Invoke(null, new MouseEventArgs(MouseButtons.Middle, 0, x, y, 0));
                        break;
                    case WM_MBUTTONDBLCLK:
                        MouseDoubleClick?.Invoke(null, new MouseEventArgs(MouseButtons.Middle, 0, x, y, 0));
                        break;
                    case WM_XBUTTONDOWN:
                        tmpnum = GetHigh((IntPtr)wParam);
                        MouseDown?.Invoke(null, new MouseEventArgs(tmpnum==1? MouseButtons.XButton1: MouseButtons.XButton2, 0, x, y, 0));
                        break;
                    case WM_XBUTTONUP:
                        tmpnum = GetHigh((IntPtr)wParam);
                        MouseUp?.Invoke(null, new MouseEventArgs(tmpnum == 1 ? MouseButtons.XButton1 : MouseButtons.XButton2, 0, x, y, 0));
                        break;
                    case WM_XBUTTONDBLCLK:
                        tmpnum = GetHigh((IntPtr)wParam);
                        MouseDoubleClick?.Invoke(null, new MouseEventArgs(tmpnum == 1 ? MouseButtons.XButton1 : MouseButtons.XButton2, 0, x, y, 0));
                        break;
                    case WM_CAPTURECHANGED:
                        CaptureChange?.Invoke((IntPtr)wParam);
                        break;
                    case WM_MOUSEWHEEL:
                        MouseWheel?.Invoke(null, new MouseEventArgs(MouseButtons.None, 0, x, y, GetHigh((IntPtr)wParam)));
                        break;
                    case WM_MOUSEHWHEEL:
                        MouseHWheel?.Invoke(null, new MouseEventArgs(MouseButtons.None, 0, x, y, GetHigh((IntPtr)wParam)));
                        break;
                    case WM_MOUSEMOVE:
                        MouseMove?.Invoke(null, new MouseEventArgs(MouseButtons.None, 0, x, y, 0));
                        break;
                    case WM_MOUSEHOVER:
                        MouseHover?.Invoke(null, new MouseEventArgs(MouseButtons.None, 0, x, y, 0));
                        break;
                    case WM_MOUSELEAVE:
                        MouseLeave?.Invoke(null, new MouseEventArgs(MouseButtons.None, 0, 0,0, 0));
                        break;
                }
            }

            return CallNextHookEx(mousehookId, nCode, wParam, lParam);
        }
        private static int GetHigh(IntPtr lParam)
        {
            return Marshal.ReadInt32(lParam, 0);
        }
        private static int GetLow(IntPtr lParam)
        {
            return Marshal.ReadInt32(lParam, 4);
        }
    }
    internal static partial class NativeMethod
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern int SetWindowsHookEx(int idHook, Hook.HookProc lpfn, int hInstance, int threadId);
        // 卸载钩子
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern bool UnhookWindowsHookEx(int idHook);
        // 继续下一个钩子
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern int CallNextHookEx(int idHook, int nCode, Int32 wParam, int lParam);
        [DllImport("kernel32.dll")]
        public static extern int GetModuleHandle(string lpModuleName);
    }
}
