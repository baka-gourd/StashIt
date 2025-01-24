using System.Diagnostics;
using System.IO.Compression;

namespace StashIt.Daemon;

internal class Program
{
    static void Main(string[] args)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("Usage: Daemon.exe <log_directory> <process_id>");
            return;
        }

        var gameDataDirectory = args[0];
        var processId = int.Parse(args[1]);

        try
        {
            // 等待主进程退出
            var mainProcess = Process.GetProcessById(processId);
            mainProcess.WaitForExit();

            StashLogs(Path.Combine(gameDataDirectory, "ModsData", ".StashIt",
                $"{DateTime.Now:yyyy-MM-dd_hh_mm_ss}.zip"), gameDataDirectory);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Daemon error: " + ex.Message);
        }
    }

    private static void EnsureDirectory(string gameDataDirectory)
    {
        var info = new DirectoryInfo(Path.Combine(gameDataDirectory, "ModsData", ".StashIt"));
        if (!info.Exists)
        {
            info.Create();
        }
    }

    private static IEnumerable<(string, DateTime)> GetLogFiles(string gameDataDirectory)
    {
        var logDirectory = Path.Combine(gameDataDirectory, "Logs");
        if (Directory.Exists(logDirectory))
        {
            var info = new DirectoryInfo(logDirectory);
            foreach (var file in info.GetFiles("*.log"))
            {
                yield return (file.FullName, file.LastWriteTime);
            }
        }

        var player = Path.Combine(gameDataDirectory, "Player.log");
        var prevPlayer = Path.Combine(gameDataDirectory, "Player-prev.log");
        if (File.Exists(player))
        {
            yield return (player, File.GetLastWriteTime(player));
        }

        if (File.Exists(prevPlayer))
        {
            yield return (prevPlayer, File.GetLastWriteTime(prevPlayer));
        }
    }

    private static string GetRelativePath(string basePath, string absolutePath)
    {
        return Path.GetRelativePath(basePath, absolutePath);
    }

    private static void StashLogs(string zipFileName, string gameDataDirectory)
    {
        EnsureDirectory(gameDataDirectory);

        using var zipStream = new FileStream(zipFileName, FileMode.Create, FileAccess.Write);
        using var zipArchive = new ZipArchive(zipStream, ZipArchiveMode.Create, leaveOpen: false);

        foreach (var (file, writeTime) in GetLogFiles(gameDataDirectory))
        {
            var relativePath = GetRelativePath(gameDataDirectory, file);

            var entry = zipArchive.CreateEntry(relativePath, CompressionLevel.SmallestSize);
            entry.LastWriteTime = writeTime;

            using var entryStream = entry.Open();
            using var fileStream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            fileStream.CopyTo(entryStream);
        }
    }
}