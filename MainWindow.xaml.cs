using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Security.Principal;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace eBayKleinanzeigenTracker
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string APP_NAME = "eBayKleinanzeigenTracker";
        private const string DONT_HIDE = "donthide";
        private RegistryKey rkApp = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
        private MinimizeToTrayInstance _minimizeToTrayInstance;
        private bool _showedConfirmMessage = false, pollRateChanged = false;
        private Tracker tracker;

        public MainWindow()
        {
            if (AppIsRunning())
            {
                System.Diagnostics.Process.GetCurrentProcess().Kill();
                return;
            }

            InitializeComponent();
            this.Closing += MainWindow_Closed;

            if (IsAdministrator()) SetSystemStart(Properties.Settings.Default.StartOnSystemStart);
            Properties.Settings.Default.StartOnSystemStart = IsSystemStart();
            Properties.Settings.Default.Save();
            bool startOnSystemStart = Properties.Settings.Default.StartOnSystemStart;
            bool startInSystemTray = Properties.Settings.Default.StartInSystemTray;
            int pollRate = Properties.Settings.Default.TrackingIntervalSec;
            _showedConfirmMessage = !Properties.Settings.Default.ShowCloseConfirmDialog;

            dgArticles.ItemsSource = SearchRequestContainer.GetInstance().SearchRequests;
            _minimizeToTrayInstance = new MinimizeToTrayInstance(this);
            if (startInSystemTray)
            {
                string[] args = Environment.GetCommandLineArgs();
                if (args.Length != 2 || !args[1].Equals(DONT_HIDE))
                {
                    SetHidden(true);
                }
            }

            cbStartOnSystemStart.IsChecked = startOnSystemStart;
            cbStartInSystemTray.IsChecked = startInSystemTray;
            txPollRate.Text = pollRate.ToString();

            cbStartOnSystemStart.Click += CheckBox_SystemStartChanged;
            cbStartInSystemTray.Click += CheckBox_StartInSystemTrayChanged;
            txPollRate.TextChanged += TextBox_PollRateChangedChanged;
            txPollRate.KeyDown += TextBox_PollRateKeyDown;
            txPollRate.LostFocus += TextBox_PollRateLostFocus;

            tracker = new Tracker(_minimizeToTrayInstance);
            tracker.Start(Properties.Settings.Default.TrackingIntervalSec);
        }

        public void SetHidden(bool hidden)
        {
            if (hidden) Hide();
            else Show();
            ShowInTaskbar = !hidden;
        }

        private bool AppIsRunning()
        {
            return System.Diagnostics.Process.GetProcessesByName(System.IO.Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetEntryAssembly().Location)).Length > 1;
        }

        private bool SetSystemStart(bool start)
        {
            if (start) rkApp.SetValue(APP_NAME, System.Reflection.Assembly.GetExecutingAssembly().Location);
            else rkApp.DeleteValue(APP_NAME, true);
            return true;
        }

        private bool IsAdministrator()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        private bool IsSystemStart()
        {
            return rkApp.GetValue(APP_NAME) != null;
        }

        private void CheckBox_SystemStartChanged(object sender, RoutedEventArgs e)
        {
            bool startOnSystemStart = cbStartOnSystemStart.IsChecked.Value;
            Properties.Settings.Default.StartOnSystemStart = startOnSystemStart;
            Properties.Settings.Default.Save();
            if (IsAdministrator()) SetSystemStart(startOnSystemStart);
            else
            {
                var exeName = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
                ProcessStartInfo startInfo = new ProcessStartInfo(exeName);
                startInfo.Verb = "runas";
                startInfo.Arguments = DONT_HIDE;
                System.Diagnostics.Process.Start(startInfo);
                Application.Current.Shutdown();
                return;
            }
            cbStartOnSystemStart.IsChecked = IsSystemStart();
        }

        private void CheckBox_StartInSystemTrayChanged(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.StartInSystemTray = cbStartInSystemTray.IsChecked.Value;
            Properties.Settings.Default.Save();
        }

        private void TextBox_PollRateChangedChanged(object sender, TextChangedEventArgs e)
        {
            pollRateChanged = true;
            TextBox_NumberChanged(sender, e);
            if (txPollRate.Text.Length == 0)
            {
                txPollRate.Text = Properties.Settings.Default.TrackingIntervalSec.ToString();
            }
        }

        private void TextBox_PollRateLostFocus(object sender, RoutedEventArgs e)
        {
            TextBox_PollRateUpdate();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (pollRateChanged)
            {
                Keyboard.ClearFocus();
                TextBox_PollRateUpdate();
            }
        }

        private void TextBox_PollRateKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Keyboard.ClearFocus();
                TextBox_PollRateUpdate();
            }
        }

        private void TextBox_PollRateUpdate()
        {
            pollRateChanged = false;
            int value = Math.Max(10, Int32.Parse(txPollRate.Text));
            Properties.Settings.Default.TrackingIntervalSec = value;
            Properties.Settings.Default.Save();
            txPollRate.Text = value.ToString();
            tracker.Start(value);
        }

        private void TextBox_PreviewKeyTextInput(object sender, TextCompositionEventArgs e)
        {
            if (e.Text.Contains("_") || e.Text.Contains(";")) e.Handled = true;
        }

        private void TextBox_PreviewNumberTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!StringTools.StringIsInt(e.Text)) e.Handled = true;
        }

        private void TextBox_NumberChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            tb.Text = StringTools.ExtractIntFromString(tb.Text);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            SearchRequestContainer.GetInstance().AddEmpty();
            dgArticles.Focus();
            dgArticles.SelectedIndex = dgArticles.Items.Count - 1;
        }

        private void MainWindow_Closed(object sender, CancelEventArgs e)
        {
            if (!_showedConfirmMessage)
            {
                ConfirmMessage confirmMessage = new ConfirmMessage();
                confirmMessage.ShowDialog();
                _showedConfirmMessage = true;
            }
            SetHidden(true);
            e.Cancel = true;
        }

    }
  

}