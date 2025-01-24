using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Colossal.IO.AssetDatabase;
using Colossal.Logging;
using Colossal.PSI.Environment;
using Game;
using Game.Modding;
using Game.Routes;
using Game.SceneFlow;
using ICSharpCode.SharpZipLib.Zip;

namespace StashIt
{
    public class StashIt : IMod
    {
        public static ILog Logger = LogManager.GetLogger($"{nameof(StashIt)}").SetShowsErrorsInUI(false);
        public static string Version = "";
        private Setting Setting;
        public int ProcessId { get; set; }

        public void OnLoad(UpdateSystem updateSystem)
        {
            Logger.Info(nameof(OnLoad));

            if (GameManager.instance.modManager.TryGetExecutableAsset(this, out var asset))
            {
                Logger.Info($"Current mod asset at {asset.path}");
                Version = asset.version.ToString();
                ProcessId = Process.GetCurrentProcess().Id;
                LaunchDaemon(Path.Combine(Path.GetDirectoryName(asset.path) ?? "", "StashIt.Daemon.exe"), $"\"{EnvPath.kUserDataPath}\" {ProcessId}");
            }

            Setting = new Setting(this);
            Setting.RegisterInOptionsUI();
            GameManager.instance.localizationManager.AddSource("en-US", new LocaleEN(Setting));


            AssetDatabase.global.LoadSettings(nameof(StashIt), Setting, new Setting(this));
        }

        public void OnDispose()
        {
            Logger.Info("Stashing logs");
            if (Setting == null) return;
            Setting.UnregisterInOptionsUI();
            Setting = null;
        }

        public static void LaunchDaemon(string daemonPath, string arguments)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = daemonPath,
                Arguments = arguments,
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                WindowStyle = ProcessWindowStyle.Hidden
            };

            Process process = new Process
            {
                StartInfo = startInfo
            };

            process.Start();
        }
    }
}