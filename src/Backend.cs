using IWshRuntimeLibrary;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using FusionMods;

namespace FusionMods.src
{
    public static class Backend
    {
        private const int SW_HIDE = 0;
        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_TOOLWINDOW = 0x00000080;
        private const int WS_EX_APPWINDOW = 0x00040000;
        static int waitingTime;

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
        

        //MAIN
        public static void StartLoader()
        {
            Process[] fiveMProcesses = Process.GetProcessesByName("FiveM");
            string servercode = Properties.Settings.Default.ServerIP;

            if (fiveMProcesses.Length > 0)
            {
                MessageBoxResult result = MessageBox.Show(
                    "Fivem is already opened! Closing?",
                    "Warning",
                    MessageBoxButton.OKCancel,
                    MessageBoxImage.Warning
                );

                if (result == MessageBoxResult.Cancel)
                {
                    return;
                }

                foreach (Process proc in fiveMProcesses)
                {
                    try
                    {
                        if (!proc.HasExited)
                        {
                            proc.CloseMainWindow();
                            if (!proc.WaitForExit(3000))
                            {
                                proc.Kill();
                                proc.WaitForExit(5000);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Fehler beim Beenden: " + ex.Message);
                    }
                }
            }

            if (Process.GetProcessesByName("steam").Length == 0)
            {
                MessageBox.Show("You must have Steam open to play");
                return;
            }

            string fivemExe = GetFiveMExecutablePath();
            if (string.IsNullOrEmpty(fivemExe))
            {
                MessageBox.Show("FiveM.exe wurde nicht gefunden.");
                return;
            }

            if (Properties.Settings.Default.AutoTeamSpeak)
            {
                StartTeamSpeak();
            }

            if (Properties.Settings.Default.PureMode)
            {
                try
                {
                    string tempoFolder = PrepareTempoFolder();
                    string shortcutPath = Path.Combine(tempoFolder, "launch_fivem.lnk");

                    var shell = new WshShell();
                    IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutPath);
                    shortcut.TargetPath = fivemExe;
                    shortcut.Arguments = "-pure_1";
                    shortcut.WindowStyle = 1;
                    shortcut.Description = "";
                    shortcut.Save();

                    var psi = new ProcessStartInfo(shortcutPath)
                    {
                        UseShellExecute = true
                    };
                    Process.Start(psi);

                    WaitForFiveMWindow();

                    Thread.Sleep(6000);
                    Process.Start(new ProcessStartInfo($"fivem://connect/cfx.re/join/{servercode}")
                    {
                        UseShellExecute = true
                    });

                    if (!int.TryParse(Properties.Settings.Default.WaitingTime, out waitingTime))
                    {
                        waitingTime = 0;
                    }

                    Thread.Sleep(waitingTime);
                    if (System.IO.File.Exists(shortcutPath))
                    {
                        System.IO.File.Delete(shortcutPath);
                    }

                    Application.Current.Shutdown();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("ERROR: " + ex.Message);
                }
            }
            else
            {
                try
                {
                    var psi = new ProcessStartInfo("explorer.exe", $"fivem://connect/cfx.re/join/{servercode}") { UseShellExecute = true };
                    Process.Start(psi);
                    Application.Current.Shutdown();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("ERROR: " + ex.Message);
                }
            }
        }

        public static string GetFiveMExecutablePath()
        {
            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string fivemPath = Path.Combine(localAppData, "FiveM", "FiveM.exe");
            return System.IO.File.Exists(fivemPath) ? fivemPath : null;
        }

        public static string PrepareTempoFolder()
        {
            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string tempoFolder = Path.Combine(localAppData, "FiveM", "tempo");

            if (!Directory.Exists(tempoFolder))
                Directory.CreateDirectory(tempoFolder);

            return tempoFolder;
        }

        public static void HideFiveMWindow()
        {
            Thread.Sleep(3000);

            foreach (Process proc in Process.GetProcessesByName("FiveM"))
            {
                IntPtr handle = proc.MainWindowHandle;
                if (handle == IntPtr.Zero) continue;

                ShowWindow(handle, SW_HIDE);

                int exStyle = GetWindowLong(handle, GWL_EXSTYLE);
                SetWindowLong(handle, GWL_EXSTYLE, (exStyle | WS_EX_TOOLWINDOW) & ~WS_EX_APPWINDOW);
            }
        }

        public static bool WaitForFiveMWindow()
        {
            const int checkIntervalMs = 500;
            var sw = Stopwatch.StartNew();

            while (sw.ElapsedMilliseconds < waitingTime)
            {
                foreach (var proc in Process.GetProcessesByName("FiveM"))
                {
                    if (proc.MainWindowHandle != IntPtr.Zero)
                        return true;
                }
                Thread.Sleep(checkIntervalMs);
            }
            return false;
        }

        private static void StartTeamSpeak()
        {
            string tsIp = Properties.Settings.Default.TeamSpeakIP;
            if (!string.IsNullOrWhiteSpace(tsIp))
            {
                try
                {
                    Process.Start(new ProcessStartInfo($"ts3server://{tsIp}")
                    {
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Fehler beim Start von TeamSpeak: " + ex.Message);
                }
            }
        }
    }
}