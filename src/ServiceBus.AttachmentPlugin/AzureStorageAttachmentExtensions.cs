namespace ServiceBus.AttachmentPlugin
{
    using Microsoft.Azure.ServiceBus;
    using Microsoft.Azure.ServiceBus.Core;

    /// <summary>Service Bus plugin to send large messages using attachments stored as Azure Storage blobs.</summary>
    public static class AzureStorageAttachmentExtensions
    {
        /// <summary>Instantiate plugin with the required configuration.</summary>
        /// <param name="client"><see cref="QueueClient"/>, <see cref="SubscriptionClient"/>, <see cref="QueueClient"/>, <see cref="MessageSender"/>, <see cref="MessageReceiver"/>, or <see cref="SessionClient"/> to register plugin with.</param>
        /// <param name="configuration"><see cref="AzureStorageAttachmentConfiguration"/> object.</param>
        public static void RegisterAzureStorageAttachmentPlugin(this ClientEntity client, AzureStorageAttachmentConfiguration configuration)
        {
            client.RegisterPlugin(new AzureStorageAttachment(configuration));
        }

        /// <summary>Initiate plugin for Receive-Only mode to retrieve attachment using SAS URI. </summary>
        /// <param name="client"><see cref="QueueClient"/>, <see cref="SubscriptionClient"/>, <see cref="QueueClient"/>, <see cref="MessageSender"/>, <see cref="MessageReceiver"/>, or <see cref="SessionClient"/> to register plugin with.</param>
        /// <param name="messagePropertyToIdentifySasUri">Message property name to be used to retrieve message SAS UI.</param>
        public static void RegisterAzureStorageAttachmentPluginForReceivingOnly(this ClientEntity client, string messagePropertyToIdentifySasUri = AzureStorageAttachmentConfigurationExtensions.DefaultMessagePropertyToIdentitySasUri)
        {
            client.RegisterPlugin(new ReceiveOnlyAzureStorageAttachment(messagePropertyToIdentifySasUri));
        }
    }
}