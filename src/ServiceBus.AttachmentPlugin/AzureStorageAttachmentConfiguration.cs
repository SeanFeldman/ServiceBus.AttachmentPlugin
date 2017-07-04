namespace ServiceBus.AttachmentPlugin
{
    using System;
    using Microsoft.Azure.ServiceBus;

    /// <summary>Runtime configuration for <see cref="AzureStorageAttachment"/> plugin.</summary>
    public class AzureStorageAttachmentConfiguration
    {
        /// <summary>Constructor to create new configuration object.</summary>
        /// <param name="connectionString"></param>
        /// <param name="containerName"></param>
        /// <param name="messagePropertyToIdentifyAttachmentBlob"></param>
        /// <param name="messageMaxSizeReachedCriteria">Default is always use attachments</param>
        public AzureStorageAttachmentConfiguration(string connectionString,
            string containerName = "attachments",
            string messagePropertyToIdentifyAttachmentBlob = "$attachment.blob",
            Func<Message, bool> messageMaxSizeReachedCriteria = null)
        {
            Guard.AgainstEmpty(nameof(containerName), containerName);
            Guard.AgainstEmpty(nameof(messagePropertyToIdentifyAttachmentBlob), messagePropertyToIdentifyAttachmentBlob);
            ConnectionString = connectionString;
            ContainerName = containerName;
            MessagePropertyToIdentifyAttachmentBlob = messagePropertyToIdentifyAttachmentBlob;
            MessageMaxSizeReachedCriteria = messageMaxSizeReachedCriteria ?? (_ => true);
        }

        /// <summary>Storage account connection string to use.</summary>
        public string ConnectionString { get; }

        /// <summary>Storage container name used for storing attachment blobs.</summary>
        public string ContainerName { get; }

        /// <summary><see cref="Message"/> property used to store attachment blob name.</summary>
        public string MessagePropertyToIdentifyAttachmentBlob { get; }

        /// <summary>
        /// Predicate used to control decision if message maximum size is exceeded or not.
        /// </summary>
        public Func<Message, bool> MessageMaxSizeReachedCriteria { get; }
    }
}