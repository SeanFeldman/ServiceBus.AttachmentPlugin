namespace ServiceBus.AttachmentPlugin.Tests
{
    using System.Threading;

    public class AzureStorageEmulatorFixture
    {
        public AzureStorageEmulatorFixture()
        {
            AzureStorageEmulatorManager.StartStorageEmulator();
            // Emulator is not started fast enough on AppVeyor
            // Microsoft.WindowsAzure.Storage.StorageException : Unable to connect to the remote server
            Thread.Sleep(1000);
        }
    }
}