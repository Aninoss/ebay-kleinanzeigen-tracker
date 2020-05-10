using System;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Windows;
using System.Windows.Forms;

namespace eBayKleinanzeigenTracker
{
    public class MinimizeToTrayInstance
    {
        private MainWindow _window;
        private NotifyIcon _notifyIcon;

        public MinimizeToTrayInstance(MainWindow window)
        {
            Debug.Assert(window != null, "window parameter is null.");
            _window = window;
            InitializeBalloonTip();
        }

        public void AddClickHandler(EventHandler eventHandler)
        {
            _notifyIcon.BalloonTipClicked += new EventHandler(eventHandler);
        }

        public void ShowBalloonTip(string title, string desc)
        {
            _notifyIcon.ShowBalloonTip(30000, title, desc, ToolTipIcon.None);
        }

        private void InitializeBalloonTip()
        {
            if (_notifyIcon == null)
            {
                _notifyIcon = new NotifyIcon();
                _notifyIcon.Icon = Icon.ExtractAssociatedIcon(Assembly.GetEntryAssembly().Location);
                _notifyIcon.Text = _window.Title;
                _notifyIcon.Visible = true;
                _notifyIcon.MouseClick += new MouseEventHandler(HandleNotifyIconOrBalloonClicked);
            }

            ContextMenu contextMenu = new ContextMenu();
            MenuItem menuItem1 = new MenuItem(), menuItem2 = new MenuItem();

            contextMenu.MenuItems.AddRange(new MenuItem[] { menuItem1, menuItem2 });

            menuItem1.Index = 0;
            menuItem1.Text = "Öffnen";
            menuItem1.Click += new System.EventHandler(HandleNotifyIconOrBalloonClicked);

            menuItem2.Index = 1;
            menuItem2.Text = "B&eenden";
            menuItem2.Click += new System.EventHandler(HandleMenu_Exit);

            _notifyIcon.ContextMenu = contextMenu;
        }

        private void HandleNotifyIconOrBalloonClicked(object sender, EventArgs e)
        {
            _window.SetHidden(false);
            if (_window.WindowState == WindowState.Minimized) _window.WindowState = WindowState.Normal;
        }

        private void HandleMenu_Exit(object sender, EventArgs e)
        {
            System.Diagnostics.Process.GetCurrentProcess().Kill();
        }

    }
}