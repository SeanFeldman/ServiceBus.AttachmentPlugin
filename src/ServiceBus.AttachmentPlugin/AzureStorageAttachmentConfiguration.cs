namespace Microsoft.Azure.ServiceBus
{
    using System;
    using Storage;
    using Storage.Auth;

    /// <summary>Runtime configuration for Azure Storage Attachment plugin.</summary>
    public class AzureStorageAttachmentConfiguration
    {
        /// <summary>Constructor to create new configuration object.</summary>
        /// <param name="connectionString"></param>
        /// <param name="containerName"></param>
        /// <param name="messagePropertyToIdentifyAttachmentBlob"></param>
        /// <param name="messageMaxSizeReachedCriteria">Default is always use attachments</param>
        public AzureStorageAttachmentConfiguration(
            string connectionString,
            string containerName = "attachments",
            string messagePropertyToIdentifyAttachmentBlob = "$attachment.blob",
            Func<Message, bool>? messageMaxSizeReachedCriteria = default)
            : this(new PlainTextConnectionStringProvider(connectionString), containerName, messagePropertyToIdentifyAttachmentBlob, messageMaxSizeReachedCriteria)
        {
        }

        /// <summary>Constructor to create new configuration object.</summary>
        /// <remarks>Container name is not required as it's included in the SharedAccessSignature.</remarks>
        /// <param name="storageCredentials"></param>
        /// <param name="blobEndpoint">Blob endpoint in the format of "https://account.blob.core.windows.net/". For the emulator the value is "http://127.0.0.1:10000/devstoreaccount1".</param>
        /// <param name="containerName"></param>
        /// <param name="messagePropertyToIdentifyAttachmentBlob"></param>
        /// <param name="messageMaxSizeReachedCriteria">Default is always use attachments</param>
        public AzureStorageAttachmentConfiguration(
            StorageCredentials storageCredentials,
            string blobEndpoint,
            string containerName = "attachments",
            string messagePropertyToIdentifyAttachmentBlob = "$attachment.blob",
            Func<Message, bool>? messageMaxSizeReachedCriteria = default)
        {
            Guard.AgainstNull(nameof(storageCredentials), storageCredentials);
            Guard.AgainstEmpty(nameof(blobEndpoint), blobEndpoint);
            Guard.AgainstEmpty(nameof(containerName), containerName);
            Guard.AgainstEmpty(nameof(messagePropertyToIdentifyAttachmentBlob), messagePropertyToIdentifyAttachmentBlob);

            StorageCredentials = storageCredentials;
            BlobEndpoint = EnsureBlobEndpointEndsWithSlash(blobEndpoint);
            ContainerName = containerName;
            MessagePropertyToIdentifyAttachmentBlob = messagePropertyToIdentifyAttachmentBlob;
            MessageMaxSizeReachedCriteria = GetMessageMaxSizeReachedCriteria(messageMaxSizeReachedCriteria);
        }

        static Uri EnsureBlobEndpointEndsWithSlash(string blobEndpoint)
        {
            if (blobEndpoint.EndsWith("/", StringComparison.OrdinalIgnoreCase))
            {
                return new Uri(blobEndpoint);
            }

            // Emulator blob endpoint doesn't end with slash
            return new Uri(blobEndpoint + "/");
        }

        /// <summary>Constructor to create new configuration object.</summary>
        /// <param name="connectionStringProvider">Provider to retrieve connection string such as <see cref="PlainTextConnectionStringProvider"/></param>
        /// <param name="containerName">Storage container name</param>
        /// <param name="messagePropertyToIdentifyAttachmentBlob">Message user property to use for blob URI</param>
        /// <param name="messageMaxSizeReachedCriteria">Default is always use attachments</param>
        public AzureStorageAttachmentConfiguration(
            IProvideStorageConnectionString connectionStringProvider,
            string containerName = "attachments",
            string messagePropertyToIdentifyAttachmentBlob = "$attachment.blob",
            Func<Message, bool>? messageMaxSizeReachedCriteria = default)
        {
            Guard.AgainstNull(nameof(connectionStringProvider), connectionStringProvider);
            Guard.AgainstEmpty(nameof(containerName), containerName);
            Guard.AgainstEmpty(nameof(messagePropertyToIdentifyAttachmentBlob), messagePropertyToIdentifyAttachmentBlob);

            var connectionString = connectionStringProvider.GetConnectionString().GetAwaiter().GetResult();
            var account = CloudStorageAccount.Parse(connectionString);

            ConnectionStringProvider = connectionStringProvider;
            StorageCredentials = account.Credentials;
            BlobEndpoint = EnsureBlobEndpointEndsWithSlash(account.BlobEndpoint.ToString());
            ContainerName = containerName;
            MessagePropertyToIdentifyAttachmentBlob = messagePropertyToIdentifyAttachmentBlob;
            MessageMaxSizeReachedCriteria = GetMessageMaxSizeReachedCriteria(messageMaxSizeReachedCriteria);
        }

        Func<Message, bool> GetMessageMaxSizeReachedCriteria(Func<Message, bool>? messageMaxSizeReachedCriteria)
        {
            if (messageMaxSizeReachedCriteria == null)
            {
                return _ => true;
            }
            return message =>
            {
                try
                {
                    return messageMaxSizeReachedCriteria(message);
                }
                catch (Exception exception)
                {
                    throw new Exception("An exception occurred when executing the MessageMaxSizeReachedCriteria delegate.", exception);
                }
            };
        }

        /// <summary>
        /// 
        /// </summary>
        public IProvideStorageConnectionString? ConnectionStringProvider { get; }

        /// <summary>
        /// 
        /// </summary>
        public string ContainerName { get; }

        internal string? MessagePropertyForBlobSasUri { get; set; }

        internal TimeSpan? BlobSasTokenValidationTime { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string MessagePropertyToIdentifyAttachmentBlob { get; }

        internal Func<Message, bool> MessageMaxSizeReachedCriteria { get; }

        internal StorageCredentials StorageCredentials { get; }

        internal bool UsingSas => StorageCredentials.IsSAS;

        internal Uri BlobEndpoint { get; }

        internal Func<Message, string> BlobNameResolver { get; set; } = message => Guid.NewGuid().ToString();
    }
}