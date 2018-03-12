namespace ServiceBus.AttachmentPlugin.Tests
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Net;
    using System.Net.NetworkInformation;
    using System.Threading;

    public class AzureStorageEmulatorFixture
    {
        public static IProvideStorageConnectionString ConnectionStringProvider = new PlainTextConnectionStringProvider("UseDevelopmentStorage=true");

        public AzureStorageEmulatorFixture()
        {
            AzureStorageEmulatorManager.StartStorageEmulator();

            // Emulator is not started fast enough on AppVeyor
            // Microsoft.WindowsAzure.Storage.StorageException : Unable to connect to the remote server

            var properties = IPGlobalProperties.GetIPGlobalProperties();
            bool emulatorStarted;

            var stopwatch = Stopwatch.StartNew();

            do
            {
                var endpoints = properties.GetActiveTcpListeners();
                emulatorStarted = endpoints.Any(x => x.Port == 10000 && Equals(x.Address, IPAddress.Loopback));
                Console.WriteLine("waiting for emulator to start...");
                Thread.Sleep(100);
            } while (emulatorStarted == false && stopwatch.Elapsed < TimeSpan.FromSeconds(60));

            if (emulatorStarted == false)
            {
                throw new Exception("Storage emulator failed to start after 10 seconds.");
            }
        }
    }
}