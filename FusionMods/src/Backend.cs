using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;

namespace FusionMods.src
{
    public static class Backend
    {
        private const int SW_HIDE = 0;
        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_TOOLWINDOW = 0x00000080;
        private const int WS_EX_APPWINDOW = 0x00040000;

        [DllImport("user32.dll")] private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        [DllImport("user32.dll")] private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")] private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        // ---------- Constants ----------
        private const string FiveMProcessName = "FiveM";
        private const string SteamProcessName = "steam";
        private const string FiveMFolderName = "FiveM";
        private const string FiveMExeName = "FiveM.exe";
        private const string PureArgs = "-pure_1";

        private static readonly string[] MasterProcessCandidates =
        {
            "FiveM_ChromeBrowser",
            "CitizenFX_ChromeBrowser",
            "FiveM_CEFHelper",
            "CitizenFX_CEFHelper"
        };

        // ===================== PUBLIC ENTRY =====================
        public static void StartLoader()
        {
            try
            {
                var serverCode = (Properties.Settings.Default.ServerIP ?? "").Trim();
                if (string.IsNullOrWhiteSpace(serverCode))
                {
                    ShowError("Kein Servercode konfiguriert.");
                    return;
                }

                if (!TryGetFiveMExePath(out var fiveMExe))
                {
                    ShowError("FiveM.exe wurde nicht gefunden.");
                    return;
                }

                if (!ConfirmAndCloseExistingFiveM()) return;

                if (!IsProcessRunning(SteamProcessName))
                {
                    ShowInfo("You must have Steam open to play");
                    return;
                }

                if (Properties.Settings.Default.AutoTeamSpeak)
                    TryStartTeamSpeak();

                var connectUri = BuildConnectUri(serverCode);

                if (Properties.Settings.Default.PureMode)
                    StartFiveMPureAndConnect(fiveMExe, connectUri);
                else
                    OpenUri(connectUri);

                Application.Current.Shutdown();
            }
            catch (Exception ex)
            {
                ShowError($"Unerwarteter Fehler: {ex.Message}");
            }
        }

        // ===================== CORE FLOW =====================
        private static void StartFiveMPureAndConnect(string fiveMExePath, string connectUri)
        {
            var start = new ProcessStartInfo(fiveMExePath, PureArgs)
            {
                UseShellExecute = true,
                WorkingDirectory = Path.GetDirectoryName(fiveMExePath) ?? ""
            };
            Process.Start(start);

            var timeout = GetWaitingTimeMs();
            if (!WaitForFiveMReady(timeout))
                throw new InvalidOperationException("FiveM wurde nicht rechtzeitig initialisiert.");

            OpenUri(connectUri);

            SleepFromSettings("WaitingTime");
        }

        // ===================== HELPERS =====================
        private static bool ConfirmAndCloseExistingFiveM()
        {
            var running = Process.GetProcessesByName(FiveMProcessName);
            if (running.Length == 0) return true;

            var result = MessageBox.Show(
                "FiveM ist bereits geöffnet! Es wird geschlossen.",
                "Warning",
                MessageBoxButton.OKCancel,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Cancel) return false;

            foreach (var proc in running)
            {
                try
                {
                    if (proc.HasExited) continue;

                    proc.CloseMainWindow();
                    if (!proc.WaitForExit(3000))
                    {
                        proc.Kill();
                        proc.WaitForExit(5000);
                    }
                }
                catch (Exception ex)
                {
                    ShowError($"Fehler beim Beenden von FiveM: {ex.Message}");
                }
            }

            return true;
        }

        private static bool TryGetFiveMExePath(out string path)
        {
            var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var candidate = Path.Combine(localAppData, FiveMFolderName, FiveMExeName);
            path = File.Exists(candidate) ? candidate : null;
            return path != null;
        }

        private static string BuildConnectUri(string serverCode)
            => $"fivem://connect/cfx.re/join/{serverCode}";

        private static bool IsProcessRunning(string processName)
            => Process.GetProcessesByName(processName).Length > 0;

        private static void OpenUri(string uri)
        {
            Process.Start(new ProcessStartInfo("explorer.exe", uri) { UseShellExecute = true });
        }

        private static void SleepFromSettings(string key)
        {
            if (int.TryParse(Properties.Settings.Default[key]?.ToString(), out var ms) && ms > 0)
                Thread.Sleep(ms);
        }

        private static int GetWaitingTimeMs()
        {
            if (!int.TryParse(Properties.Settings.Default.WaitingTime, out var ms) || ms <= 0)
                ms = 15000; // Default: 15s
            return ms;
        }

        private static bool WaitForFiveMReady(int timeoutMs)
        {
            var sw = Stopwatch.StartNew();

            if (!WaitUntil(() => Process.GetProcessesByName(FiveMProcessName).Any(), timeoutMs))
                return false;

            bool MasterExists() =>
                Process.GetProcesses().Any(p =>
                    MasterProcessCandidates.Any(c =>
                        p.ProcessName.Equals(c, StringComparison.OrdinalIgnoreCase) ||
                        p.ProcessName.Contains(c, StringComparison.OrdinalIgnoreCase)));

            if (WaitUntil(MasterExists, Remaining(sw, timeoutMs)))
                return true;

            bool AnyWindow() =>
                Process.GetProcessesByName(FiveMProcessName).Any(p => p.MainWindowHandle != IntPtr.Zero);

            return WaitUntil(AnyWindow, Remaining(sw, timeoutMs));
        }

        private static bool WaitUntil(Func<bool> predicate, int timeoutMs, int pollMs = 250)
        {
            var sw = Stopwatch.StartNew();
            while (sw.ElapsedMilliseconds < timeoutMs)
            {
                if (predicate()) return true;
                Thread.Sleep(pollMs);
            }
            return false;
        }

        private static int Remaining(Stopwatch sw, int totalMs)
        {
            var left = totalMs - (int)sw.ElapsedMilliseconds;
            return left > 0 ? left : 0;
        }

        public static void HideFiveMWindow()
        {
            Thread.Sleep(3000);
            foreach (var proc in Process.GetProcessesByName(FiveMProcessName))
            {
                var handle = proc.MainWindowHandle;
                if (handle == IntPtr.Zero) continue;

                ShowWindow(handle, SW_HIDE);
                var exStyle = GetWindowLong(handle, GWL_EXSTYLE);
                SetWindowLong(handle, GWL_EXSTYLE, (exStyle | WS_EX_TOOLWINDOW) & ~WS_EX_APPWINDOW);
            }
        }

        // ---- TeamSpeak ----
        private static void TryStartTeamSpeak()
        {
            var tsIp = Properties.Settings.Default.TeamSpeakIP;
            if (string.IsNullOrWhiteSpace(tsIp)) return;

            try
            {
                Process.Start(new ProcessStartInfo($"ts3server://{tsIp}") { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                ShowError($"Fehler beim Start von TeamSpeak: {ex.Message}");
            }
        }

        // ---- UI Helpers ----
        private static void ShowError(string message)
            => MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);

        private static void ShowInfo(string message)
            => MessageBox.Show(message, "Info", MessageBoxButton.OK, MessageBoxImage.Information);
    }
}
