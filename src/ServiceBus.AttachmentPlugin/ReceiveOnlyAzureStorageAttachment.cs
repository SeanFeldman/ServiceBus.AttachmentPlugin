namespace ServiceBus.AttachmentPlugin
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using Azure;
    using Azure.Storage.Blobs.Specialized;
    using Microsoft.Azure.ServiceBus;
    using Microsoft.Azure.ServiceBus.Core;

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

            var blob = new BlockBlobClient(new Uri(userProperties[messagePropertyToIdentifySasUri].ToString() 
                                                   ?? throw new Exception($"Value of {nameof(messagePropertyToIdentifySasUri)} `{messagePropertyToIdentifySasUri}` was null.")));
            try
            {
                await blob.GetPropertiesAsync().ConfigureAwait(false);
            }
            catch (RequestFailedException exception)
            {
                throw new Exception($"Blob with name '{blob.Name}' under container '{blob.BlobContainerName}' cannot be found.", exception);
            }

            await using var memory = new MemoryStream();
            await blob.DownloadToAsync(memory).ConfigureAwait(false);
            message.Body = memory.ToArray();
            return message;
        }
    }
}