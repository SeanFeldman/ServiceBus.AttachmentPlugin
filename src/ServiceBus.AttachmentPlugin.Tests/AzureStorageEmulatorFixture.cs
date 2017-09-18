namespace ServiceBus.AttachmentPlugin.Tests
{
    using System;
    using System.Threading;

    public class AzureStorageEmulatorFixture : IDisposable
    {
        public AzureStorageEmulatorFixture()
        {
            AzureStorageEmulatorManager.StartStorageEmulator();
            // Emulator is not started fast enough on AppVeyor
            // Microsoft.WindowsAzure.Storage.StorageException : Unable to connect to the remote server
            Thread.Sleep(1000);
        }

        public void Dispose()
        {
        }
    }
}