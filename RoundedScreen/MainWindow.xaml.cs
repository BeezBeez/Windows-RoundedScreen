using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace RoundedScreen
{
    public partial class MainWindow : Window
    {
        public const int WS_EX_TRANSPARENT = 0x00000020;
        public const int GWL_EXSTYLE = (-20);
        public const int WS_EX_TOOLWINDOW = 0x00000080;
        private const string RegistryKeyPath = "SOFTWARE\\RoundedScreen";
        private const string CornerSizeValueName = "CornerSize";
        private const string RoundingModeValueName = "RoundingMode"; // 0=Simple(circle), 1=Smooth(squircle)
        private const int DefaultCornerSize = 28;
        private const int DefaultRoundingMode = 1; // Smooth squircle by default
        private bool _allowClose = false;

        public enum RoundingMode
        {
            Simple = 0,
            Smooth = 1,
        }

        [DllImport("user32.dll")]
        public static extern int GetWindowLong(IntPtr hwnd, int index);

        [DllImport("user32.dll")]
        public static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);

        public MainWindow()
        {
            InitializeComponent();
            this.SetStartup();
            // React to display/resolution/orientation changes
            SystemEvents.DisplaySettingsChanged += OnDisplaySettingsChanged;
            SystemParameters.StaticPropertyChanged += SystemParameters_StaticPropertyChanged;
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

        protected override void OnClosed(EventArgs e)
        {
            // Unsubscribe from static events to avoid memory leaks
            SystemEvents.DisplaySettingsChanged -= OnDisplaySettingsChanged;
            SystemParameters.StaticPropertyChanged -= SystemParameters_StaticPropertyChanged;
            base.OnClosed(e);
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

        private void OnDisplaySettingsChanged(object sender, EventArgs e)
        {
            // Ensure UI-thread update
            Dispatcher.Invoke(ReapplyRoundingAndLayout);
        }

        private void SystemParameters_StaticPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SystemParameters.PrimaryScreenWidth) ||
                e.PropertyName == nameof(SystemParameters.PrimaryScreenHeight))
            {
                Dispatcher.Invoke(ReapplyRoundingAndLayout);
            }
        }

        private void ReapplyRoundingAndLayout()
        {
            this.WindowStartupLocation = WindowStartupLocation.Manual;
            this.Left = 0;
            this.Top = 0;
            this.Width = System.Windows.SystemParameters.PrimaryScreenWidth;
            this.Height = System.Windows.SystemParameters.PrimaryScreenHeight;

            int size = ReadCornerSize();
            ApplyCornerSize(size);

            this.InvalidateVisual();
            this.UpdateLayout();
        }

        private static Geometry CreateSquircleCornerGeometry(double size, int samples = 36)
        {
            // Quarter superellipse (a=b=size) with exponent n≈4 for Apple-like squircle
            // x = a * cos^(2/n)(t), y = b * sin^(2/n)(t), t ∈ [0, π/2]
            // Build a composite geometry: Outer rectangle (0,0,size,size) MINUS the inner squircle curve
            // This ensures the filled area is only the corner wedge, not the inner curve area.

            if (samples < 8) samples = 8;

            var geo = new StreamGeometry { FillRule = FillRule.EvenOdd };
            using (var ctx = geo.Open())
            {
                // 1) Outer rectangle figure
                ctx.BeginFigure(new Point(0, 0), isFilled: true, isClosed: true);
                ctx.LineTo(new Point(size, 0), isStroked: false, isSmoothJoin: false);
                ctx.LineTo(new Point(size, size), isStroked: false, isSmoothJoin: false);
                ctx.LineTo(new Point(0, size), isStroked: false, isSmoothJoin: false);

                // 2) Inner squircle figure (hole)
                ctx.BeginFigure(new Point(size, 0), isFilled: true, isClosed: true);

                double n = 4.0; // squircle exponent
                for (int i = 1; i <= samples; i++)
                {
                    double t = (Math.PI / 2.0) * (i / (double)samples);
                    double ct = Math.Cos(t);
                    double st = Math.Sin(t);
                    double x = size * Math.Pow(Math.Abs(ct), 2.0 / n);
                    double y = size * Math.Pow(Math.Abs(st), 2.0 / n);
                    ctx.LineTo(new Point(x, y), isStroked: false, isSmoothJoin: true);
                }

                ctx.LineTo(new Point(0, size), isStroked: false, isSmoothJoin: false);
                ctx.LineTo(new Point(0, 0), isStroked: false, isSmoothJoin: false);
                ctx.LineTo(new Point(size, 0), isStroked: false, isSmoothJoin: false);
            }

            geo.Freeze();
            return geo;
        }

        private static Geometry CreateCircularCornerGeometry(double size, int samples = 0)
        {
            // EvenOdd: outer square (0,0,size,size) minus a quarter circle of radius 'size' from (size,0) to (0,size)
            var geo = new StreamGeometry { FillRule = FillRule.EvenOdd };
            using (var ctx = geo.Open())
            {
                // Outer square
                ctx.BeginFigure(new Point(0, 0), isFilled: true, isClosed: true);
                ctx.LineTo(new Point(size, 0), false, false);
                ctx.LineTo(new Point(size, size), false, false);
                ctx.LineTo(new Point(0, size), false, false);

                // Inner quarter circle (hole): start at (size,0), arc to (0,size)
                ctx.BeginFigure(new Point(size, 0), isFilled: true, isClosed: true);
                var arc = new ArcSegment(new Point(0, size), new Size(size, size), 0, false, SweepDirection.Clockwise, isStroked: false);
                ctx.ArcTo(arc.Point, arc.Size, arc.RotationAngle, arc.IsLargeArc, arc.SweepDirection, isStroked: false, isSmoothJoin: true);
                ctx.LineTo(new Point(0, 0), false, false);
                ctx.LineTo(new Point(size, 0), false, false);
            }
            geo.Freeze();
            return geo;
        }

        public void ApplyCornerSize(int size)
        {
            if (size < 4) size = 4;
            if (size > 64) size = 64;

            // Update path sizes
            pathCornerTL.Width = size;
            pathCornerTL.Height = size;
            pathCornerTR.Width = size;
            pathCornerTR.Height = size;
            pathCornerBL.Width = size;
            pathCornerBL.Height = size;
            pathCornerBR.Width = size;
            pathCornerBR.Height = size;

            // Choose geometry based on rounding mode
            var mode = ReadRoundingMode();
            Geometry geo = mode == RoundingMode.Smooth
                ? CreateSquircleCornerGeometry(size)
                : CreateCircularCornerGeometry(size);
            pathCornerTL.Data = geo;
            pathCornerTR.Data = geo;
            pathCornerBL.Data = geo;
            pathCornerBR.Data = geo;
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

        public void SaveRoundingMode(RoundingMode mode)
        {
            using (var key = Registry.CurrentUser.CreateSubKey(RegistryKeyPath))
            {
                key.SetValue(RoundingModeValueName, (int)mode, RegistryValueKind.DWord);
            }
        }

        public RoundingMode ReadRoundingMode()
        {
            using (var key = Registry.CurrentUser.CreateSubKey(RegistryKeyPath))
            {
                object value = key.GetValue(RoundingModeValueName, DefaultRoundingMode);
                try
                {
                    int v = Convert.ToInt32(value);
                    if (v != 0 && v != 1) return (RoundingMode)DefaultRoundingMode;
                    return (RoundingMode)v;
                }
                catch
                {
                    return (RoundingMode)DefaultRoundingMode;
                }
            }
        }

        public void AllowClose()
        {
            _allowClose = true;
        }
    }
}
