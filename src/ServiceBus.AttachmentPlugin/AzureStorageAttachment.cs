namespace ServiceBus.AttachmentPlugin
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.ServiceBus;
    using Microsoft.Azure.ServiceBus.Core;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;

    class AzureStorageAttachment : ServiceBusPlugin
    {
        SemaphoreSlim semaphore = new SemaphoreSlim(1);
        internal const string MessageId = "_MessageId";
        internal const string ValidUntilUtc = "_ValidUntilUtc";
        internal const string DateFormat = "yyyy-MM-dd HH:mm:ss:ffffff Z";

        AzureStorageAttachmentConfiguration configuration;

        public AzureStorageAttachment(AzureStorageAttachmentConfiguration configuration)
        {
            Guard.AgainstNull(nameof(configuration), configuration);
            this.configuration = configuration;
        }

        public override string Name => nameof(AzureStorageAttachment);

        internal static Func<DateTime> DateTimeFunc = () => DateTime.UtcNow;

        public override async Task<Message> BeforeMessageSend(Message message)
        {
            if (AttachmentBlobAssociated(message.UserProperties))
            {
                return message;
            }

            if (!configuration.MessageMaxSizeReachedCriteria(message))
            {
                return message;
            }

            var containerUri = new Uri($"{configuration.BlobEndpoint}{configuration.ContainerName}");
            var container = new CloudBlobContainer(containerUri, configuration.StorageCredentials);

            // Will only work for Shared Key or Account SAS. For Container SAS will throw an exception.
            if (! await container.ExistsAsync().ConfigureAwait(false))
            {
                await container.CreateIfNotExistsAsync().ConfigureAwait(false);
            }

            var blobUri = new Uri($"{containerUri}/{Guid.NewGuid().ToString()}");
            var blob = new CloudBlockBlob(blobUri, configuration.StorageCredentials);

            SetValidMessageId(blob, message.MessageId);
            SetValidUntil(blob, message.TimeToLive);

            await blob.UploadFromByteArrayAsync(message.Body, 0, message.Body.Length).ConfigureAwait(false);

            message.Body = null;
            message.UserProperties[configuration.MessagePropertyToIdentifyAttachmentBlob] = blob.Name;

            if (!configuration.BlobSasTokenValidationTime.HasValue)
            {
                return message;
            }

            // TODO: only possible if connection string is used
            // configuration.StorageCredentials.IsSharedKey
            var sasUri = TokenGenerator.GetBlobSasUri(blob, configuration.BlobSasTokenValidationTime.Value);
            message.UserProperties[configuration.MessagePropertyForBlobSasUri] = sasUri;
            return message;
        }

        bool AttachmentBlobAssociated(IDictionary<string, object> messageUserProperties) =>
            messageUserProperties.TryGetValue(configuration.MessagePropertyToIdentifyAttachmentBlob, out var _);

        static void SetValidMessageId(ICloudBlob blob, string messageId)
        {
            if (!string.IsNullOrWhiteSpace(messageId))
            {
                blob.Metadata[MessageId] = messageId;
            }
        }

        static void SetValidUntil(ICloudBlob blob, TimeSpan timeToBeReceived)
        {
            if (timeToBeReceived == TimeSpan.MaxValue)
            {
                return;
            }

            var validUntil = DateTimeFunc().Add(timeToBeReceived);
            blob.Metadata[ValidUntilUtc] = validUntil.ToString(DateFormat);
        }

        public override async Task<Message> AfterMessageReceive(Message message)
        {
            var userProperties = message.UserProperties;

            if (!userProperties.ContainsKey(configuration.MessagePropertyToIdentifyAttachmentBlob))
            {
                return message;
            }

            CloudBlockBlob blob;

            if (configuration.MessagePropertyForBlobSasUri != null && userProperties.ContainsKey(configuration.MessagePropertyForBlobSasUri))
            {
                blob = new CloudBlockBlob(new Uri(userProperties[configuration.MessagePropertyForBlobSasUri].ToString()));
            }
            else
            {
                var blobName = (string)userProperties[configuration.MessagePropertyToIdentifyAttachmentBlob];
                var blobUri = new Uri($"{configuration.BlobEndpoint}{configuration.ContainerName}/{blobName}");
                blob = new CloudBlockBlob(blobUri, configuration.StorageCredentials);
            }

            try
            {
                await blob.FetchAttributesAsync().ConfigureAwait(false);
            }
            catch (StorageException exception)
            {
                throw new Exception($"Blob with name '{blob.Name}' under container '{blob.Container.Name}' cannot be found."
                    + $" Check {nameof(AzureStorageAttachmentConfiguration)}.{nameof(AzureStorageAttachmentConfiguration.ContainerName)} or"
                    + $" {nameof(AzureStorageAttachmentConfiguration)}.{nameof(AzureStorageAttachmentConfiguration.MessagePropertyToIdentifyAttachmentBlob)} for correct values.", exception);
            }
            var fileByteLength = blob.Properties.Length;
            var bytes = new byte[fileByteLength];
            await blob.DownloadToByteArrayAsync(bytes, 0).ConfigureAwait(false);
            message.Body = bytes;
            return message;
        }
    }
}
