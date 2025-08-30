using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;

namespace RussianHelper
{
    public class GlobalHookService : IDisposable
    {
        private readonly LowLevelMouseProc _mouseProc;
        private IntPtr _hookId = IntPtr.Zero;
        private readonly Action<System.Drawing.Point> _onMouseHover;
        private readonly Action<System.Drawing.Point> _onMouseMove;
        private bool _isHooked = false;

        public GlobalHookService(Action<System.Drawing.Point> onMouseHover, Action<System.Drawing.Point> onMouseMove)
        {
            _mouseProc = HookCallback;
            _onMouseHover = onMouseHover;
            _onMouseMove = onMouseMove;
        }

        public void StartHook()
        {
            if (_isHooked) return;
            
            _hookId = SetHook(_mouseProc);
            _isHooked = true;
        }

        public void StopHook()
        {
            if (!_isHooked) return;
            
            UnhookWindowsHookEx(_hookId);
            _isHooked = false;
        }

        private IntPtr SetHook(LowLevelMouseProc proc)
        {
            using (var curProcess = System.Diagnostics.Process.GetCurrentProcess())
            using (var curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_MOUSE_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                var hookStruct = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));
                
                switch ((int)wParam)
                {
                    case WM_MOUSEMOVE:
                        _onMouseMove?.Invoke(new System.Drawing.Point(hookStruct.pt.x, hookStruct.pt.y));
                        break;
                }
            }
            
            return CallNextHookEx(_hookId, nCode, wParam, lParam);
        }

        public void Dispose()
        {
            StopHook();
        }

        // Win32 API declarations
        private const int WH_MOUSE_LL = 14;
        private const int WM_MOUSEMOVE = 0x0200;

        private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int x;
            public int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MSLLHOOKSTRUCT
        {
            public POINT pt;
            public uint mouseData;
            public uint flags;
            public uint time;
            public IntPtr dwExtraInfo;
        }
    }
}
