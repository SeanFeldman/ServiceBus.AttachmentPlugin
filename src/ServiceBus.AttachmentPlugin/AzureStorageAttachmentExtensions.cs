namespace Microsoft.Azure.ServiceBus
{
    using System.Threading.Tasks;
    using Core;
    using global::ServiceBus.AttachmentPlugin;

    /// <summary>Service Bus plugin to send large messages using attachments stored as Azure Storage blobs.</summary>
    public static class AzureStorageAttachmentExtensions
    {
        /// <summary>Instantiate plugin with the required configuration.</summary>
        /// <param name="client"><see cref="QueueClient"/>, <see cref="SubscriptionClient"/>, <see cref="QueueClient"/>, <see cref="MessageSender"/>, <see cref="MessageReceiver"/>, or <see cref="SessionClient"/> to register plugin with.</param>
        /// <param name="configuration"><see cref="AzureStorageAttachmentConfiguration"/> object.</param>
        /// <returns>Registered plugin as <see cref="ServiceBusPlugin"/>.</returns>
        public static ServiceBusPlugin RegisterAzureStorageAttachmentPlugin(this ClientEntity client, AzureStorageAttachmentConfiguration configuration)
        {
            ServiceBusPlugin plugin;

            if (!configuration.UsingContainerSas)
            {
                plugin = new AzureStorageAttachment(configuration);
            }
            else
            {
                plugin = new SasBasedAzureStorageAttachment(configuration);
            }

            client.RegisterPlugin(plugin);

            return plugin;
        }

        /// <summary>Initiate plugin for Receive-Only mode to retrieve attachments using SAS URI. </summary>
        /// <param name="client"><see cref="QueueClient"/>, <see cref="SubscriptionClient"/>, <see cref="QueueClient"/>, <see cref="MessageSender"/>, <see cref="MessageReceiver"/>, or <see cref="SessionClient"/> to register plugin with.</param>
        /// <param name="messagePropertyToIdentifySasUri">Message property name to be used to retrieve message SAS UI.</param>
        /// <returns>Registered plugin as <see cref="ServiceBusPlugin"/>.</returns>
        public static ServiceBusPlugin RegisterAzureStorageAttachmentPluginForReceivingOnly(this ClientEntity client, string messagePropertyToIdentifySasUri = AzureStorageAttachmentConfigurationExtensions.DefaultMessagePropertyToIdentitySasUri)
        {
            ServiceBusPlugin plugin = new ReceiveOnlyAzureStorageAttachment(messagePropertyToIdentifySasUri);

            client.RegisterPlugin(plugin);

            return plugin;
        }
    }

    internal class SasBasedAzureStorageAttachment : ServiceBusPlugin
    {
        readonly AzureStorageAttachmentConfiguration configuration;

        public SasBasedAzureStorageAttachment(AzureStorageAttachmentConfiguration configuration)
        {
            this.configuration = configuration;
        }

        // TODO: should the name be the same name for all permutations? looks like a bug
        public override string Name => nameof(SasBasedAzureStorageAttachment);

        // TODO: need to review this. looks like a bug. If plugin is failing, should throw.
        //public override bool ShouldContinueOnException { get; }

        public override Task<Message> BeforeMessageSend(Message message)
        {
            return base.BeforeMessageSend(message);
        }

        public override Task<Message> AfterMessageReceive(Message message)
        {
            return base.AfterMessageReceive(message);
        }
    }
}