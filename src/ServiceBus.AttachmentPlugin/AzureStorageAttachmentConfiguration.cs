namespace Microsoft.Azure.ServiceBus
{
    using System;

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
            Func<Message, bool> messageMaxSizeReachedCriteria = null)
            : this(new PlainTextConnectionStringProvider(connectionString), containerName, messagePropertyToIdentifyAttachmentBlob, messageMaxSizeReachedCriteria)
        {
        }

        /// <summary>Constructor to create new configuration object.</summary>
        /// <remarks>Container name is not required as it's included in the SharedAccessSignature.</remarks>
        /// <param name="sharedAccessSignature"></param>
        /// <param name="messagePropertyToIdentifyAttachmentBlob"></param>
        /// <param name="messageMaxSizeReachedCriteria">Default is always use attachments</param>
        public AzureStorageAttachmentConfiguration(
            SharedAccessSignature sharedAccessSignature,
            string messagePropertyToIdentifyAttachmentBlob = "$attachment.blob",
            Func<Message, bool> messageMaxSizeReachedCriteria = null)
            : this(new PlainTextConnectionStringProvider(sharedAccessSignature.QueryString), null, messagePropertyToIdentifyAttachmentBlob, messageMaxSizeReachedCriteria)
        {
            this.SharedAccessSignature = sharedAccessSignature;
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
            Func<Message, bool> messageMaxSizeReachedCriteria = null)
        {
            Guard.AgainstEmpty(nameof(containerName), containerName);
            Guard.AgainstEmpty(nameof(messagePropertyToIdentifyAttachmentBlob), messagePropertyToIdentifyAttachmentBlob);
            ConnectionStringProvider = connectionStringProvider;
            ContainerName = containerName;
            MessagePropertyToIdentifyAttachmentBlob = messagePropertyToIdentifyAttachmentBlob;
            MessageMaxSizeReachedCriteria = GetMessageMaxSizeReachedCriteria(messageMaxSizeReachedCriteria);
        }

        Func<Message, bool> GetMessageMaxSizeReachedCriteria(Func<Message, bool> messageMaxSizeReachedCriteria)
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

        internal IProvideStorageConnectionString ConnectionStringProvider { get; }

        internal string ContainerName { get; }

        internal string MessagePropertyForSasUri { get; set; }

        internal TimeSpan? SasTokenValidationTime { get; set; }

        internal string MessagePropertyToIdentifyAttachmentBlob { get; }

        internal Func<Message, bool> MessageMaxSizeReachedCriteria { get; }

        internal SharedAccessSignature SharedAccessSignature { get; }

        internal bool UsingContainerSas => SharedAccessSignature != null;
    }
}