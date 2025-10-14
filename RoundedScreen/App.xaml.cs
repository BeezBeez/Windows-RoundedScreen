using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

namespace RoundedScreen
{
    /// <summary>
    /// Logique d'interaction pour App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        private NotifyIcon _trayIcon;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            _trayIcon = new NotifyIcon();
            _trayIcon.Icon = System.Drawing.Icon.ExtractAssociatedIcon(System.Windows.Forms.Application.ExecutablePath);
            _trayIcon.Visible = true;
            _trayIcon.Text = "RoundedScreen";

            var menu = new ContextMenuStrip();

            void AddSizeItem(string text, int size)
            {
                var item = new ToolStripMenuItem(text);
                item.Click += (s, a) => ApplySize(size);
                menu.Items.Add(item);
            }

            menu.Items.Add(new ToolStripLabel("Corner size"));
            menu.Items.Add(new ToolStripSeparator());
            int[] presets = new int[] { 4, 6, 8, 10, 12, 14, 16, 18, 20, 22, 24, 28, 32, 40, 48, 56, 64 };
            foreach (var size in presets)
            {
                AddSizeItem($"{size} px", size);
            }
            menu.Items.Add(new ToolStripSeparator());

            // Rounding mode section
            menu.Items.Add(new ToolStripLabel("Rounding mode"));
            var simpleItem = new ToolStripMenuItem("Simple (Circle)") { CheckOnClick = true };
            var smoothItem = new ToolStripMenuItem("Smooth (Squircle)") { CheckOnClick = true };

            // Initialize checked state from registry
            var wndInit = Current.Windows.OfType<MainWindow>().FirstOrDefault();
            var currentMode = wndInit?.ReadRoundingMode() ?? RoundedScreen.MainWindow.RoundingMode.Smooth;
            simpleItem.Checked = currentMode == RoundedScreen.MainWindow.RoundingMode.Simple;
            smoothItem.Checked = currentMode == RoundedScreen.MainWindow.RoundingMode.Smooth;

            void ApplyMode(RoundedScreen.MainWindow.RoundingMode mode)
            {
                var wnd = Current.Windows.OfType<MainWindow>().FirstOrDefault();
                if (wnd != null)
                {
                    wnd.SaveRoundingMode(mode);
                    int size = wnd.ReadCornerSize();
                    wnd.ApplyCornerSize(size);
                }
            }

            simpleItem.Click += (s, a) =>
            {
                simpleItem.Checked = true;
                smoothItem.Checked = false;
                ApplyMode(RoundedScreen.MainWindow.RoundingMode.Simple);
            };
            smoothItem.Click += (s, a) =>
            {
                simpleItem.Checked = false;
                smoothItem.Checked = true;
                ApplyMode(RoundedScreen.MainWindow.RoundingMode.Smooth);
            };

            menu.Items.Add(simpleItem);
            menu.Items.Add(smoothItem);
            menu.Items.Add(new ToolStripSeparator());

            var exitItem = new ToolStripMenuItem("Exit");
            exitItem.Click += (s, a) => ExitApplication();
            menu.Items.Add(exitItem);

            _trayIcon.ContextMenuStrip = menu;
        }

        private void ApplySize(int size)
        {
            var wnd = Current.Windows.OfType<MainWindow>().FirstOrDefault();
            if (wnd != null)
            {
                wnd.ApplyCornerSize(size);
                wnd.SaveCornerSize(size);
            }
        }

        private void ExitApplication()
        {
            var wnd = Current.Windows.OfType<MainWindow>().FirstOrDefault();
            if (wnd != null)
            {
                wnd.AllowClose();
                wnd.Close();
            }
            _trayIcon.Visible = false;
            _trayIcon.Dispose();
            Shutdown();
        }
    }
}
