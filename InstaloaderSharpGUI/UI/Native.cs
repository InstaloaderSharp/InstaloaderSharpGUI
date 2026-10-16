using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace InstaloaderSharpGUI.UI
{
    /// <summary>Win32 interop used for dark mode, cue banners, taskbar progress and power management.</summary>
    internal static class Native
    {
        public const int WM_PAINT = 0x000F;
        public const int WM_NCPAINT = 0x0085;
        public const int WM_SETREDRAW = 0x000B;
        public const int EM_SETCUEBANNER = 0x1501;
        public const int CB_SETCUEBANNER = 0x1703;

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        [DllImport("uxtheme.dll", CharSet = CharSet.Unicode)]
        private static extern int SetWindowTheme(IntPtr hWnd, string pszSubAppName, string pszSubIdList);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, string lParam);

        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern bool FlashWindowEx(ref FLASHWINFO pwfi);

        [DllImport("kernel32.dll")]
        private static extern uint SetThreadExecutionState(uint esFlags);

        [StructLayout(LayoutKind.Sequential)]
        private struct FLASHWINFO
        {
            public uint cbSize;
            public IntPtr hwnd;
            public uint dwFlags;
            public uint uCount;
            public uint dwTimeout;
        }

        /// <summary>Asks DWM to draw a dark title bar (Windows 10 1809+ / Windows 11).</summary>
        public static void UseDarkTitleBar(IntPtr handle)
        {
            try
            {
                int on = 1;
                // DWMWA_USE_IMMERSIVE_DARK_MODE is 20 on Windows 10 20H1+, 19 on earlier builds.
                if (DwmSetWindowAttribute(handle, 20, ref on, sizeof(int)) != 0)
                    DwmSetWindowAttribute(handle, 19, ref on, sizeof(int));
            }
            catch (Exception)
            {
                // Not supported on this OS - keep the default title bar.
            }
        }

        /// <summary>Applies the dark Explorer theme, which gives dark scroll bars to standard controls.</summary>
        public static void UseDarkScrollBars(Control control)
        {
            try
            {
                if (control.IsHandleCreated)
                    SetWindowTheme(control.Handle, "DarkMode_Explorer", null);
                else
                    control.HandleCreated += (s, e) => SetWindowTheme(((Control)s).Handle, "DarkMode_Explorer", null);
            }
            catch (Exception)
            {
            }
        }

        public static void SetCueBanner(TextBox textBox, string text)
        {
            if (textBox.IsHandleCreated)
                SendMessage(textBox.Handle, EM_SETCUEBANNER, (IntPtr)1, text ?? "");
        }

        public static void FlashUntilFocused(Form form)
        {
            try
            {
                var info = new FLASHWINFO
                {
                    hwnd = form.Handle,
                    dwFlags = 0x3 | 0xC, // FLASHW_ALL | FLASHW_TIMERNOFG
                    uCount = uint.MaxValue,
                    dwTimeout = 0
                };
                info.cbSize = (uint)Marshal.SizeOf(info);
                FlashWindowEx(ref info);
            }
            catch (Exception)
            {
            }
        }

        /// <summary>Keeps the computer awake while a download runs.</summary>
        public static void PreventSleep(bool prevent)
        {
            const uint ES_CONTINUOUS = 0x80000000;
            const uint ES_SYSTEM_REQUIRED = 0x00000001;
            try
            {
                SetThreadExecutionState(prevent ? ES_CONTINUOUS | ES_SYSTEM_REQUIRED : ES_CONTINUOUS);
            }
            catch (Exception)
            {
            }
        }

        public static void SuspendDrawing(Control c)
        {
            SendMessage(c.Handle, WM_SETREDRAW, IntPtr.Zero, IntPtr.Zero);
        }

        public static void ResumeDrawing(Control c)
        {
            SendMessage(c.Handle, WM_SETREDRAW, (IntPtr)1, IntPtr.Zero);
            c.Invalidate(true);
        }
    }

    public enum TaskbarState
    {
        None = 0,
        Indeterminate = 0x1,
        Normal = 0x2,
        Error = 0x4,
        Paused = 0x8
    }

    /// <summary>Shows download progress on the taskbar button (ITaskbarList3).</summary>
    internal static class TaskbarProgress
    {
        [ComImport, Guid("ea1afb91-9e28-4b86-90e9-9e9f8a5eefaf"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface ITaskbarList3
        {
            [PreserveSig] int HrInit();
            [PreserveSig] int AddTab(IntPtr hwnd);
            [PreserveSig] int DeleteTab(IntPtr hwnd);
            [PreserveSig] int ActivateTab(IntPtr hwnd);
            [PreserveSig] int SetActiveAlt(IntPtr hwnd);
            [PreserveSig] int MarkFullscreenWindow(IntPtr hwnd, [MarshalAs(UnmanagedType.Bool)] bool fFullscreen);
            [PreserveSig] int SetProgressValue(IntPtr hwnd, ulong ullCompleted, ulong ullTotal);
            [PreserveSig] int SetProgressState(IntPtr hwnd, TaskbarState state);
        }

        [ComImport, Guid("56fdf344-fd6d-11d0-958a-006097c9a090"), ClassInterface(ClassInterfaceType.None)]
        private class TaskbarInstance
        {
        }

        private static ITaskbarList3 _instance;
        private static bool _failed;

        private static ITaskbarList3 Instance
        {
            get
            {
                if (_instance == null && !_failed)
                {
                    try
                    {
                        _instance = (ITaskbarList3)new TaskbarInstance();
                        _instance.HrInit();
                    }
                    catch (Exception)
                    {
                        _failed = true;
                    }
                }
                return _instance;
            }
        }

        public static void SetState(Form form, TaskbarState state)
        {
            ITaskbarList3 tb = Instance;
            if (tb != null && form.IsHandleCreated)
                tb.SetProgressState(form.Handle, state);
        }

        public static void SetValue(Form form, long completed, long total)
        {
            ITaskbarList3 tb = Instance;
            if (tb != null && form.IsHandleCreated && total > 0)
                tb.SetProgressValue(form.Handle, (ulong)Math.Max(0, Math.Min(completed, total)), (ulong)total);
        }
    }
}
