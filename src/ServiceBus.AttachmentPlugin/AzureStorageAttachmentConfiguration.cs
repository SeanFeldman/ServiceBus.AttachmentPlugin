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

        internal string ConnectionString { get; }

        internal string ContainerName { get; }

        internal string MessagePropertyForSasUri { get; set; }

        internal TimeSpan? SasTokenValidationTime { get; set; }

        internal string MessagePropertyToIdentifyAttachmentBlob { get; }

        internal Func<Message, bool> MessageMaxSizeReachedCriteria { get; }
    }
}