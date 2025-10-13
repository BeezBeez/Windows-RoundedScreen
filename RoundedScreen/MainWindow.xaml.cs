using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace RoundedScreen
{
    public partial class MainWindow : Window
    {
        public const int WS_EX_TRANSPARENT = 0x00000020;
        public const int GWL_EXSTYLE = (-20);
        public const int WS_EX_TOOLWINDOW = 0x00000080;
        private const string RegistryKeyPath = "SOFTWARE\\RoundedScreen";
        private const string CornerSizeValueName = "CornerSize";
        private const int DefaultCornerSize = 16;
        private bool _allowClose = false;

        [DllImport("user32.dll")]
        public static extern int GetWindowLong(IntPtr hwnd, int index);

        [DllImport("user32.dll")]
        public static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);

        public MainWindow()
        {
            InitializeComponent();
            this.SetStartup();
        }

        private void SetStartup()
        {
            RegistryKey rk = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            rk.SetValue("RoundedScreen", Process.GetCurrentProcess().MainModule.FileName);
            Debug.WriteLine(Process.GetCurrentProcess().MainModule.FileName);
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            IntPtr hwnd = new WindowInteropHelper(this).Handle;
            int extendedStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
            SetWindowLong(hwnd, GWL_EXSTYLE, extendedStyle | WS_EX_TRANSPARENT | WS_EX_TOOLWINDOW);
        }

        private void WndRoundedScreen_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!_allowClose)
            {
                e.Cancel = true;
            }
        }

        private void WndRoundedScreen_LostFocus(object sender, RoutedEventArgs e)
        {
            this.Activate();
            this.Topmost = true; 
            this.Topmost = false; 
            this.Focus();      
        }

        private void WndRoundedScreen_Loaded(object sender, RoutedEventArgs e)
        {
            Point location = this.PointToScreen(new Point(0, 0));
            this.WindowStartupLocation = WindowStartupLocation.Manual;

            this.Width = System.Windows.SystemParameters.PrimaryScreenWidth;
            this.Height = System.Windows.SystemParameters.PrimaryScreenHeight;

            int size = ReadCornerSize();
            ApplyCornerSize(size);
        }

        public void ApplyCornerSize(int size)
        {
            if (size < 4) size = 4;
            if (size > 64) size = 64;

            imgCornerTL.Width = size;
            imgCornerTL.Height = size;
            imgCornerTR.Width = size;
            imgCornerTR.Height = size;
            imgCornerBL.Width = size;
            imgCornerBL.Height = size;
            imgCornerBR.Width = size;
            imgCornerBR.Height = size;
        }

        public void SaveCornerSize(int size)
        {
            using (var key = Registry.CurrentUser.CreateSubKey(RegistryKeyPath))
            {
                key.SetValue(CornerSizeValueName, size, RegistryValueKind.DWord);
            }
        }

        public int ReadCornerSize()
        {
            using (var key = Registry.CurrentUser.CreateSubKey(RegistryKeyPath))
            {
                object value = key.GetValue(CornerSizeValueName, DefaultCornerSize);
                try
                {
                    return Convert.ToInt32(value);
                }
                catch
                {
                    return DefaultCornerSize;
                }
            }
        }

        public void AllowClose()
        {
            _allowClose = true;
        }
    }
}
