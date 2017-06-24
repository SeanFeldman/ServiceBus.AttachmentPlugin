namespace ServiceBus.AttachmentPlugin.Tests
{
    using System;
    public class AzureStorageEmulatorFixture : IDisposable
    {
        public AzureStorageEmulatorFixture()
        {
            AzureStorageEmulatorManager.StartStorageEmulator();
        }

        public void Dispose()
        {
        }
    }
}