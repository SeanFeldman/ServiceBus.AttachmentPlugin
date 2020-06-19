namespace ServiceBus.AttachmentPlugin
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using Azure;
    using Azure.Storage.Blobs;
    using Azure.Storage.Blobs.Specialized;
    using Microsoft.Azure.ServiceBus;
    using Microsoft.Azure.ServiceBus.Core;

    class AzureStorageAttachment : ServiceBusPlugin
    {
        internal const string MessageId = "_MessageId";
        internal const string ValidUntilUtc = "_ValidUntilUtc";
        internal const string DateFormat = "yyyy-MM-dd HH:mm:ss:ffffff Z";

        AzureStorageAttachmentConfiguration configuration;

        public AzureStorageAttachment(AzureStorageAttachmentConfiguration configuration)
        {
            Guard.AgainstNull(nameof(configuration), configuration);
            this.configuration = configuration;
        }

        public override bool ShouldContinueOnException { get; } = false;

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
            var container = new BlobContainerClient(containerUri, configuration.StorageSharedKeyCredentials);

            try
            {
                // Will only work for Shared Key or Account SAS. For Container SAS will throw an exception.
                if (! await container.ExistsAsync().ConfigureAwait(false))
                {
                    await container.CreateIfNotExistsAsync().ConfigureAwait(false);
                }
            }
            catch (RequestFailedException)
            {
                // swallow in case a container SAS is used
            }

            var blobName = configuration.BlobNameResolver(message);
            var blobUri = new Uri($"{containerUri}/{blobName}");
            var blob = new BlockBlobClient(blobUri, configuration.StorageSharedKeyCredentials);

            var metadata = new Dictionary<string, string>();
            SetValidMessageId(metadata, message.MessageId);
            SetValidUntil(metadata, message.TimeToLive);

            using var memory = new MemoryStream(message.Body);
            await blob.UploadAsync(memory, metadata:metadata).ConfigureAwait(false);

            message.Body = configuration.BodyReplacer(message);
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

        static void SetValidMessageId(Dictionary<string, string> metadata, string messageId)
        {
            if (!string.IsNullOrWhiteSpace(messageId))
            {
                metadata[MessageId] = messageId;
            }
        }

        static void SetValidUntil(Dictionary<string, string> metadata, TimeSpan timeToBeReceived)
        {
            if (timeToBeReceived == TimeSpan.MaxValue)
            {
                return;
            }

            var validUntil = DateTimeFunc().Add(timeToBeReceived);
            metadata[ValidUntilUtc] = validUntil.ToString(DateFormat);
        }

        public override async Task<Message> AfterMessageReceive(Message message)
        {
            var userProperties = message.UserProperties;

            if (!userProperties.TryGetValue(configuration.MessagePropertyToIdentifyAttachmentBlob, out var blobNameObject))
            {
                return message;
            }

            var blob = BuildBlob(userProperties, blobNameObject);

            try
            {
                using var memory = new MemoryStream();
                await blob.DownloadToAsync(memory).ConfigureAwait(false);
                message.Body = memory.ToArray();
                return message;
            }
            catch (RequestFailedException exception)
            {
                throw new Exception($"Blob with name '{blob.Name}' under container '{blob.BlobContainerName}' cannot be found."
                    + $" Check {nameof(AzureStorageAttachmentConfiguration)}.{nameof(AzureStorageAttachmentConfiguration.ContainerName)} or"
                    + $" {nameof(AzureStorageAttachmentConfiguration)}.{nameof(AzureStorageAttachmentConfiguration.MessagePropertyToIdentifyAttachmentBlob)} for correct values.", exception);
            }
        }

        BlockBlobClient BuildBlob(IDictionary<string, object> userProperties, object blobNameObject)
        {
            if (configuration.MessagePropertyForBlobSasUri != null)
            {
                if (userProperties.TryGetValue(configuration.MessagePropertyForBlobSasUri, out var propertyForBlobSasUri))
                {
                    return new BlockBlobClient(new Uri((string)propertyForBlobSasUri));
                }
            }

            var blobName = (string) blobNameObject;
            var blobUri = new Uri($"{configuration.BlobEndpoint}{configuration.ContainerName}/{blobName}");
            return new BlockBlobClient(blobUri, configuration.StorageSharedKeyCredentials);
        }
    }
}
