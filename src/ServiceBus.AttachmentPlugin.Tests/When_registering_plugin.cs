namespace ServiceBus.AttachmentPlugin.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Azure.ServiceBus;
    using Microsoft.Azure.ServiceBus.Core;
    using Xunit;

    public class When_registering
    {
        [Fact]
        public void Should_get_back_object_for_full_plugin()
        {
            var client = new FakeClientEntity("fake", string.Empty, RetryPolicy.Default);

            var configuration = new AzureStorageAttachmentConfiguration(
                connectionStringProvider: AzureStorageEmulatorFixture.ConnectionStringProvider, containerName: "attachments", messagePropertyToIdentifyAttachmentBlob: "attachment-id");

            ServiceBusPlugin registeredPlugin = AzureStorageAttachmentExtensions.RegisterAzureStorageAttachmentPlugin(client, configuration);

            Assert.Equal(registeredPlugin, client.RegisteredPlugins.First());
            Assert.IsAssignableFrom<ServiceBusPlugin>(registeredPlugin);
        }

        [Fact]
        public void Should_get_back_object_for_receive_only_plugin()
        {
            var client = new FakeClientEntity("fake", string.Empty, RetryPolicy.Default);

            ServiceBusPlugin registeredPlugin = AzureStorageAttachmentExtensions.RegisterAzureStorageAttachmentPluginForReceivingOnly(client, "mySasUriProperty");

            Assert.Equal(registeredPlugin, client.RegisteredPlugins.First());
            Assert.IsAssignableFrom<ServiceBusPlugin>(registeredPlugin);
        }

        class FakeClientEntity : ClientEntity
        {
            public FakeClientEntity(string clientTypeName, string postfix, RetryPolicy retryPolicy) : base(clientTypeName, postfix, retryPolicy)
            {
                RegisteredPlugins = new List<ServiceBusPlugin>();
            }

            public override void RegisterPlugin(ServiceBusPlugin serviceBusPlugin)
            {
                RegisteredPlugins.Add(serviceBusPlugin);
            }

            public override void UnregisterPlugin(string serviceBusPluginName)
            {
                var toRemove = RegisteredPlugins.First(x => x.Name == serviceBusPluginName);
                RegisteredPlugins.Remove(toRemove);
            }

            public override TimeSpan OperationTimeout { get; set; }
            public override IList<ServiceBusPlugin> RegisteredPlugins { get; }

            protected override Task OnClosingAsync()
            {
                throw new NotImplementedException();
            }
        }
    }
}