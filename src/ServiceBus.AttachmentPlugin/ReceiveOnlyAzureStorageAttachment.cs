namespace ServiceBus.AttachmentPlugin
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Azure.ServiceBus;
    using Microsoft.Azure.ServiceBus.Core;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;

    class ReceiveOnlyAzureStorageAttachment : ServiceBusPlugin
    {
        string messagePropertyToIdentifySasUri;

        public ReceiveOnlyAzureStorageAttachment(string messagePropertyToIdentifySasUri)
        {
            this.messagePropertyToIdentifySasUri = messagePropertyToIdentifySasUri;
        }

        public override bool ShouldContinueOnException { get; } = false;

        public override string Name { get; } = nameof(AzureStorageAttachment);

        public override async Task<Message> AfterMessageReceive(Message message)
        {
            var userProperties = message.UserProperties;
            if (!userProperties.ContainsKey(messagePropertyToIdentifySasUri))
            {
                return message;
            }

            var blob = new CloudBlockBlob(new Uri(userProperties[messagePropertyToIdentifySasUri].ToString()));
            try
            {
                await blob.FetchAttributesAsync().ConfigureAwait(false);
            }
            catch (StorageException exception)
            {
                throw new Exception($"Blob with name '{blob.Name}' under container '{blob.Container.Name}' cannot be found.", exception);
            }
            var fileByteLength = blob.Properties.Length;
            var bytes = new byte[fileByteLength];
            await blob.DownloadToByteArrayAsync(bytes, 0).ConfigureAwait(false);
            message.Body = bytes;
            return message;
        }
    }
}