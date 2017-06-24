using Microsoft.Azure.ServiceBus.Core;

namespace ServiceBus.AttachmentPlugin
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using Microsoft.Azure.ServiceBus;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;

    /// <summary>Service Bus plugin to send large messages using attachments stored as Azure Storage blobs.</summary>
    public class AzureStorageAttachment : ServiceBusPlugin
    {
        internal const string MessageId = "_MessageId";
        internal const string ValidUntilUtc = "_ValidUntilUtc";
        internal const string DateFormat = "yyyy-MM-dd HH:mm:ss:ffffff Z";

        Lazy<CloudBlobClient> client;
        AzureStorageAttachmentConfiguration configuration;

        /// <summary>Instantiate plugin with the required configuration.</summary>
        public AzureStorageAttachment(AzureStorageAttachmentConfiguration configuration)
        {
            var account = CloudStorageAccount.Parse(configuration.ConnectionString);
            client = new Lazy<CloudBlobClient>(() => account.CreateCloudBlobClient());
            this.configuration = configuration;
        }

        /// <inheritdoc />
        public override string Name => nameof(AzureStorageAttachment);

        internal static Func<DateTime> DateTimeFunc = () => DateTime.UtcNow;

        /// <inheritdoc />
        public override async Task<Message> BeforeMessageSend(Message message)
        {
            if (!configuration.MessageMaxSizeReachedCriteria(message))
            {
                return message;
            }

            var container = client.Value.GetContainerReference(configuration.ContainerName);
            await container.CreateIfNotExistsAsync().ConfigureAwait(false);
            var blob = container.GetBlockBlobReference(Guid.NewGuid().ToString());
            SetValidMessageId(blob, message.MessageId);
            SetValidUntil(blob, message.TimeToLive);

            using (var stream = new MemoryStream(message.Body))
            {
                await blob.UploadFromStreamAsync(stream).ConfigureAwait(false);
            }

            message.Body = null;
            message.UserProperties[configuration.MessagePropertyToIdentifyAttachmentBlob] = blob.Name;
            return message;
        }

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

        /// <inheritdoc />
        public override async Task<Message> AfterMessageReceive(Message message)
        {
            if (!message.UserProperties.ContainsKey(configuration.MessagePropertyToIdentifyAttachmentBlob))
            {
                return message;
            }

            var container = client.Value.GetContainerReference(configuration.ContainerName);
            await container.CreateIfNotExistsAsync().ConfigureAwait(false);
            var blob = container.GetBlockBlobReference(message.UserProperties[configuration.MessagePropertyToIdentifyAttachmentBlob].ToString());
            using (var stream = new MemoryStream())
            {
                await blob.DownloadToStreamAsync(stream).ConfigureAwait(false);
                message.Body = stream.ToArray();
            }
            return message;
        }
    }
}
