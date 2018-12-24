namespace ServiceBus.AttachmentPlugin.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Azure.ServiceBus;
    using Microsoft.Azure.ServiceBus.Core;
    using Xunit;

    public class When_reusing_plugin : IClassFixture<AzureStorageEmulatorFixture>
    {
        [Fact]
        public void Should_throw_if_plugin_was_disposed()
        {
            var client = new FakeClientEntity("fake", string.Empty, RetryPolicy.Default);

            var configuration = new AzureStorageAttachmentConfiguration(
                connectionStringProvider: AzureStorageEmulatorFixture.ConnectionStringProvider, containerName: "attachments", messagePropertyToIdentifyAttachmentBlob: "attachment-id");

            var registeredPlugin = AzureStorageAttachmentExtensions.RegisterAzureStorageAttachmentPlugin(client, configuration);

            ((IDisposable)registeredPlugin).Dispose();

            Assert.ThrowsAsync<ObjectDisposedException>(() => registeredPlugin.BeforeMessageSend(null));
            Assert.ThrowsAsync<ObjectDisposedException>(() => registeredPlugin.AfterMessageReceive(null));
        }

        [Fact]
        public async Task Should_not_throw_if_plugin_was_not_disposed()
        {
            var client = new FakeClientEntity("fake", string.Empty, RetryPolicy.Default);

            var configuration = new AzureStorageAttachmentConfiguration(
                connectionStringProvider: AzureStorageEmulatorFixture.ConnectionStringProvider, containerName: "attachments", messagePropertyToIdentifyAttachmentBlob: "attachment-id");

            var registeredPlugin = AzureStorageAttachmentExtensions.RegisterAzureStorageAttachmentPlugin(client, configuration);

            var client2 = new FakeClientEntity("fake2", string.Empty, RetryPolicy.Default);

            client2.RegisterPlugin(registeredPlugin);

            var message = new Message(new byte[] {});
            await registeredPlugin.BeforeMessageSend(message);
            await registeredPlugin.AfterMessageReceive(message);
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

            public override string Path { get; }
            public override TimeSpan OperationTimeout { get; set; }
            public override ServiceBusConnection ServiceBusConnection { get; }
            public override IList<ServiceBusPlugin> RegisteredPlugins { get; }

            protected override Task OnClosingAsync()
            {
                throw new NotImplementedException();
            }
        }
    }
}