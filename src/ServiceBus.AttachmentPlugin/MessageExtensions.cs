namespace Microsoft.Azure.ServiceBus
{
    using global::ServiceBus.AttachmentPlugin;
    using System.Threading.Tasks;

    /// <summary>Extension methods for working with attachments without registering plugin.</summary>
    public static class MessageExtensions
    {
        /// <summary>Upload attachment to Azure Storage blob without registering plugin.</summary>
        /// <param name="message"><see cref="Message"/></param>
        /// <param name="configuration"><see cref="AzureStorageAttachmentConfiguration"/> object.</param>
        /// <returns><see cref="Message"/> with body uploaded to Azure Storage blob.</returns>
        public static async Task<Message> UploadAzureStorageAttachment(this Message message, AzureStorageAttachmentConfiguration configuration)
        {
            var plugin = new AzureStorageAttachment(configuration);
            return await plugin.BeforeMessageSend(message).ConfigureAwait(false);
        }

        /// <summary>Download attachment from Azure Storage blob without registering plugin, using configuration object.</summary>
        /// <param name="message"><see cref="Message"/></param>
        /// <param name="configuration"><see cref="AzureStorageAttachmentConfiguration"/> object.</param>
        /// <returns><see cref="Message"/> with body downloaded from Azure Storage blob.</returns>
        public static async Task<Message> DownloadAzureStorageAttachment(this Message message, AzureStorageAttachmentConfiguration configuration)
        {
            var plugin = new AzureStorageAttachment(configuration);
            return await plugin.AfterMessageReceive(message).ConfigureAwait(false);
        }

        /// <summary>Download attachment from Azure Storage blob without registering plugin, using message property to identify SAS URI.</summary>
        /// <param name="message"><see cref="Message"/></param>
        /// <param name="messagePropertyToIdentifySasUri">Message property which contains the SAS URI used to fetch message body from blob.</param>
        /// <returns><see cref="Message"/> with body downloaded from Azure Storage blob.</returns>
        public static async Task<Message> DownloadAzureStorageAttachment(this Message message, string messagePropertyToIdentifySasUri = AzureStorageAttachmentConfigurationExtensions.DefaultMessagePropertyToIdentitySasUri)
        {
            var plugin = new ReceiveOnlyAzureStorageAttachment(messagePropertyToIdentifySasUri);
            return await plugin.AfterMessageReceive(message).ConfigureAwait(false);
        }
    }
}