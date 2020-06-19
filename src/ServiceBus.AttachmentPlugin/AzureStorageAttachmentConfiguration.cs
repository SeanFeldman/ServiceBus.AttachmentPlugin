namespace Microsoft.Azure.ServiceBus
{
    using System;
    using global::Azure.Storage;

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
            string containerName = AzureStorageAttachmentConfigurationConstants.DefaultContainerName,
            string messagePropertyToIdentifyAttachmentBlob = AzureStorageAttachmentConfigurationConstants.DefaultMessagePropertyToIdentifyAttachmentBlob,
            Func<Message, bool>? messageMaxSizeReachedCriteria = default)
        {
            Guard.AgainstNull(nameof(connectionString), connectionString);
            Guard.AgainstEmpty(nameof(containerName), containerName);
            Guard.AgainstEmpty(nameof(messagePropertyToIdentifyAttachmentBlob), messagePropertyToIdentifyAttachmentBlob);

            ContainerName = containerName;
            MessagePropertyToIdentifyAttachmentBlob = messagePropertyToIdentifyAttachmentBlob;
            MessageMaxSizeReachedCriteria = GetMessageMaxSizeReachedCriteria(messageMaxSizeReachedCriteria);

            StorageSharedKeyCredentials = null;
            BlobEndpoint = null;
        }

        /// <summary>Constructor to create new configuration object.</summary>
        /// <remarks>Container name is not required as it's included in the SharedAccessSignature.</remarks>
        /// <param name="storageSharedKeyCredentials"></param>
        /// <param name="blobEndpoint">Blob endpoint in the format of "https://account.blob.core.windows.net/". For the emulator the value is "http://127.0.0.1:10000/devstoreaccount1".</param>
        /// <param name="containerName"></param>
        /// <param name="messagePropertyToIdentifyAttachmentBlob"></param>
        /// <param name="messageMaxSizeReachedCriteria">Default is always use attachments</param>
        public AzureStorageAttachmentConfiguration(
            StorageSharedKeyCredential storageSharedKeyCredentials,
            string blobEndpoint,
            string containerName = AzureStorageAttachmentConfigurationConstants.DefaultContainerName,
            string messagePropertyToIdentifyAttachmentBlob = AzureStorageAttachmentConfigurationConstants.DefaultMessagePropertyToIdentifyAttachmentBlob,
            Func<Message, bool>? messageMaxSizeReachedCriteria = default)
        {
            Guard.AgainstNull(nameof(storageSharedKeyCredentials), storageSharedKeyCredentials);
            Guard.AgainstEmpty(nameof(blobEndpoint), blobEndpoint);
            Guard.AgainstEmpty(nameof(containerName), containerName);
            Guard.AgainstEmpty(nameof(messagePropertyToIdentifyAttachmentBlob), messagePropertyToIdentifyAttachmentBlob);

            StorageSharedKeyCredentials = storageSharedKeyCredentials;
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
            string containerName = AzureStorageAttachmentConfigurationConstants.DefaultContainerName,
            string messagePropertyToIdentifyAttachmentBlob = AzureStorageAttachmentConfigurationConstants.DefaultMessagePropertyToIdentifyAttachmentBlob,
            Func<Message, bool>? messageMaxSizeReachedCriteria = default)
        {
            throw new NotImplementedException();
            // Guard.AgainstNull(nameof(connectionStringProvider), connectionStringProvider);
            // Guard.AgainstEmpty(nameof(containerName), containerName);
            // Guard.AgainstEmpty(nameof(messagePropertyToIdentifyAttachmentBlob), messagePropertyToIdentifyAttachmentBlob);
            //
            // var connectionString = connectionStringProvider.GetConnectionString().GetAwaiter().GetResult();
            // var account = CloudStorageAccount.Parse(connectionString);
            //
            // ConnectionStringProvider = connectionStringProvider;
            // StorageSharedKeyCredentials = account.Credentials;
            // BlobEndpoint = EnsureBlobEndpointEndsWithSlash(account.BlobEndpoint.ToString());
            // ContainerName = containerName;
            // MessagePropertyToIdentifyAttachmentBlob = messagePropertyToIdentifyAttachmentBlob;
            // MessageMaxSizeReachedCriteria = GetMessageMaxSizeReachedCriteria(messageMaxSizeReachedCriteria);
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

        internal IProvideStorageConnectionString? ConnectionStringProvider { get; }

        internal string ContainerName { get; }

        internal string? MessagePropertyForBlobSasUri { get; set; }

        internal TimeSpan? BlobSasTokenValidationTime { get; set; }

        internal string MessagePropertyToIdentifyAttachmentBlob { get; }

        internal Func<Message, bool> MessageMaxSizeReachedCriteria { get; }

        internal StorageSharedKeyCredential? StorageSharedKeyCredentials { get; }

        internal Uri? BlobEndpoint { get; }

        // internal bool UsingSas => StorageSharedKeyCredentials.IsSAS;


        internal Func<Message, string> BlobNameResolver { get; set; } = message => Guid.NewGuid().ToString();

        internal Func<Message, byte[]?> BodyReplacer { get; set; } = message => null;
    }
}