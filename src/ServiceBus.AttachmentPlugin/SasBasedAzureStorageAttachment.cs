namespace Microsoft.Azure.ServiceBus
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Threading.Tasks;
    using WindowsAzure.Storage;
    using WindowsAzure.Storage.Blob;
    using Core;
    using global::ServiceBus.AttachmentPlugin;

    class SasBasedAzureStorageAttachment : ServiceBusPlugin
    {
        readonly AzureStorageAttachmentConfiguration configuration;

        public SasBasedAzureStorageAttachment(AzureStorageAttachmentConfiguration configuration)
        {
            this.configuration = configuration;
        }

        // TODO: should the name be the same name for all permutations? looks like a bug
        // https://github.com/SeanFeldman/ServiceBus.AttachmentPlugin/issues/76
        public override string Name => nameof(SasBasedAzureStorageAttachment);

        // TODO: need to review this. looks like a bug. If plugin is failing, should throw.
        // https://github.com/SeanFeldman/ServiceBus.AttachmentPlugin/issues/75
        //public override bool ShouldContinueOnException { get; }

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

            var blobName = Guid.NewGuid().ToString();
            var uri = new Uri($"{configuration.SharedAccessSignature.ContainerUri}/{blobName}{configuration.SharedAccessSignature.QueryString}");
            var blob = new CloudBlockBlob(uri);

            SetValidMessageId(blob, message.MessageId);
            SetValidUntil(blob, message.TimeToLive);

            await blob.UploadFromByteArrayAsync(message.Body, 0, message.Body.Length).ConfigureAwait(false);

            message.Body = null;
            message.UserProperties[configuration.MessagePropertyToIdentifyAttachmentBlob] = blob.Name;

            return message;
        }

        // TODO: duplicated from AzureStorageAttachment
        bool AttachmentBlobAssociated(IDictionary<string, object> messageUserProperties) =>
            messageUserProperties.TryGetValue(configuration.MessagePropertyToIdentifyAttachmentBlob, out var _);

        // TODO: duplicated from AzureStorageAttachment
        static void SetValidMessageId(ICloudBlob blob, string messageId)
        {
            if (!string.IsNullOrWhiteSpace(messageId))
            {
                blob.Metadata[AzureStorageAttachment.MessageId] = messageId;
            }
        }

        // TODO: duplicated from AzureStorageAttachment
        static void SetValidUntil(ICloudBlob blob, TimeSpan timeToBeReceived)
        {
            if (timeToBeReceived == TimeSpan.MaxValue)
            {
                return;
            }

            var validUntil = DateTimeFunc().Add(timeToBeReceived);
            blob.Metadata[AzureStorageAttachment.ValidUntilUtc] = validUntil.ToString(AzureStorageAttachment.DateFormat);
        }

        // TODO: duplicated from AzureStorageAttachment
        internal static Func<DateTime> DateTimeFunc = () => DateTime.UtcNow;

        public override async Task<Message> AfterMessageReceive(Message message)
        {
            var userProperties = message.UserProperties;

            if (!userProperties.ContainsKey(configuration.MessagePropertyToIdentifyAttachmentBlob))
            {
                return message;
            }

            var blobName = (string)userProperties[configuration.MessagePropertyToIdentifyAttachmentBlob];
            var uri = new Uri($"{configuration.SharedAccessSignature.ContainerUri}/{blobName}{configuration.SharedAccessSignature.QueryString}");
            var blob = new CloudBlockBlob(uri);

            try
            {
                await blob.FetchAttributesAsync().ConfigureAwait(false);
            }
            catch (StorageException exception)
            {
                // TODO: should Forbidden be handled differently?
                if (exception.RequestInformation?.HttpStatusCode == (int)HttpStatusCode.Forbidden)
                {
                    Console.WriteLine("forbidden");
                }
                Console.WriteLine(exception.RequestInformation?.ExtendedErrorInformation?.ErrorCode);
                Console.WriteLine(exception.Message);

                // TODO: different exception is needed here - the sas uri should be reported back
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