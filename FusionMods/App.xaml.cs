using System.Configuration;
using System.Data;
using System.Windows;
using System;
using System.Linq;

using AppProps = FusionMods.Properties;

namespace FusionMods
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            bool? devArg = ParseDevModeArg(e.Args);
            if (devArg.HasValue)
            {
                global::FusionMods.Properties.Settings.Default.DevMode = devArg.Value;
                global::FusionMods.Properties.Settings.Default.Save();
            }
        }

        private static bool? ParseDevModeArg(string[] args)
        {
            foreach (var a in args)
            {
                if (a.StartsWith("--devmode", StringComparison.OrdinalIgnoreCase))
                {
                    var parts = a.Split(new[] { '=', ':' }, 2, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 1) return true;
                    if (bool.TryParse(parts[1], out var val)) return val;
                }
            }
            return null;
        }
    }

}
