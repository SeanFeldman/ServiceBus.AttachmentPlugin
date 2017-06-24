namespace ServiceBus.AttachmentPlugin.Tests
{
    using System.Diagnostics;
    using System.Linq;

    // Start/stop azure storage emulator from code:
    // http://stackoverflow.com/questions/7547567/how-to-start-azure-storage-emulator-from-within-a-program
    public static class AzureStorageEmulatorManager
    {
        const string AzureStorageEmulatorPath = @"C:\Program Files (x86)\Microsoft SDKs\Azure\Storage Emulator\AzureStorageEmulator.exe";
        const string Win7ProcessName = "WAStorageEmulator";
        const string Win8ProcessName = "WASTOR~1";
        const string Win10ProcessName = "AzureStorageEmulator";

        static readonly ProcessStartInfo startStorageEmulator = new ProcessStartInfo
        {
            FileName = AzureStorageEmulatorPath,
            Arguments = "start",
            UseShellExecute = false
        };

        static readonly ProcessStartInfo stopStorageEmulator = new ProcessStartInfo
        {
            FileName = AzureStorageEmulatorPath,
            Arguments = "stop",
        };

        static Process GetProcess()
        {
            return Process.GetProcessesByName(Win10ProcessName).FirstOrDefault()
                   ?? Process.GetProcessesByName(Win8ProcessName).FirstOrDefault()
                   ?? Process.GetProcessesByName(Win7ProcessName).FirstOrDefault();
        }

        internal static bool IsProcessStarted()
        {
            return GetProcess() != null;
        }

        public static void StartStorageEmulator()
        {
            if (IsProcessStarted())
            {
                return;
            }
            using (var process = Process.Start(startStorageEmulator))
            {
                process.WaitForExit();
            }
        }

        public static void StopStorageEmulator()
        {
            using (var process = Process.Start(stopStorageEmulator))
            {
                process.WaitForExit();
            }
        }
    }
}