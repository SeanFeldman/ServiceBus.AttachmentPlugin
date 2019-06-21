namespace Microsoft.Azure.ServiceBus
{
    using Core;
    using global::ServiceBus.AttachmentPlugin;

    /// <summary>Service Bus plugin to send large messages using attachments stored as Azure Storage blobs.</summary>
    public static class AzureStorageAttachmentExtensions
    {
        /// <summary>Instantiate plugin with the required configuration.</summary>
        /// <param name="client"><see cref="ClientEntity"/>, <see cref="SubscriptionClient"/>, <see cref="QueueClient"/>, <see cref="MessageSender"/>, <see cref="MessageReceiver"/>, or <see cref="SessionClient"/> to register plugin with.</param>
        /// <param name="configuration"><see cref="AzureStorageAttachmentConfiguration"/> object.</param>
        /// <returns>Registered plugin as <see cref="ServiceBusPlugin"/>.</returns>
        public static ServiceBusPlugin RegisterAzureStorageAttachmentPlugin(this ClientEntity client, AzureStorageAttachmentConfiguration configuration)
        {
            var plugin = new AzureStorageAttachment(configuration);

            client.RegisterPlugin(plugin);

            return plugin;
        }

        /// <summary>Initiate plugin for Receive-Only mode to retrieve attachments using SAS URI. </summary>
        /// <param name="client"><see cref="ClientEntity"/>, <see cref="SubscriptionClient"/>, <see cref="QueueClient"/>, <see cref="MessageSender"/>, <see cref="MessageReceiver"/>, or <see cref="SessionClient"/> to register plugin with.</param>
        /// <param name="messagePropertyToIdentifySasUri">Message property name to be used to retrieve message SAS UI.</param>
        /// <returns>Registered plugin as <see cref="ServiceBusPlugin"/>.</returns>
        public static ServiceBusPlugin RegisterAzureStorageAttachmentPluginForReceivingOnly(this ClientEntity client, string messagePropertyToIdentifySasUri = AzureStorageAttachmentConfigurationExtensions.DefaultMessagePropertyToIdentitySasUri)
        {
            var plugin = new ReceiveOnlyAzureStorageAttachment(messagePropertyToIdentifySasUri);

            client.RegisterPlugin(plugin);

            return plugin;
        }

        /// <summary>Instantiate plugin with the required configuration.</summary>
        /// <param name="client"><see cref="IClientEntity"/>, <see cref="SubscriptionClient"/>, <see cref="QueueClient"/>, <see cref="MessageSender"/>, <see cref="MessageReceiver"/>, or <see cref="SessionClient"/> to register plugin with.</param>
        /// <param name="configuration"><see cref="AzureStorageAttachmentConfiguration"/> object.</param>
        /// <returns>Registered plugin as <see cref="ServiceBusPlugin"/>.</returns>
        public static ServiceBusPlugin RegisterAzureStorageAttachmentPlugin(this IClientEntity client, AzureStorageAttachmentConfiguration configuration)
        {
            var plugin = new AzureStorageAttachment(configuration);

            client.RegisterPlugin(plugin);

            return plugin;
        }

        /// <summary>Initiate plugin for Receive-Only mode to retrieve attachments using SAS URI. </summary>
        /// <param name="client"><see cref="IClientEntity"/>, <see cref="SubscriptionClient"/>, <see cref="QueueClient"/>, <see cref="MessageSender"/>, <see cref="MessageReceiver"/>, or <see cref="SessionClient"/> to register plugin with.</param>
        /// <param name="messagePropertyToIdentifySasUri">Message property name to be used to retrieve message SAS UI.</param>
        /// <returns>Registered plugin as <see cref="ServiceBusPlugin"/>.</returns>
        public static ServiceBusPlugin RegisterAzureStorageAttachmentPluginForReceivingOnly(this IClientEntity client, string messagePropertyToIdentifySasUri = AzureStorageAttachmentConfigurationExtensions.DefaultMessagePropertyToIdentitySasUri)
        {
            var plugin = new ReceiveOnlyAzureStorageAttachment(messagePropertyToIdentifySasUri);

            client.RegisterPlugin(plugin);

            return plugin;
        }
    }
}