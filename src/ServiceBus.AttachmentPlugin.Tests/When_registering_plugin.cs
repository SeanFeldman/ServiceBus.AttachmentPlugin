namespace ServiceBus.AttachmentPlugin.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Azure.ServiceBus;
    using Microsoft.Azure.ServiceBus.Core;
    using Microsoft.WindowsAzure.Storage.Auth;
    using Xunit;

    public class When_registering_plugin : IClassFixture<AzureStorageEmulatorFixture>
    {
        const string ConnectionString = "AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;DefaultEndpointsProtocol=http;BlobEndpoint=http://127.0.0.1:10000/devstoreaccount1";

        [Fact]
        public void Should_get_back_AzureStorageAttachment_for_connection_string_based_configuration()
        {
            var client = new FakeClientEntity("fake", string.Empty, RetryPolicy.Default);

            var azureStorageAttachmentConfiguration = new AzureStorageAttachmentConfiguration(ConnectionString);

            var registeredPlugin = AzureStorageAttachmentExtensions.RegisterAzureStorageAttachmentPlugin(client, azureStorageAttachmentConfiguration);

            Assert.Equal(registeredPlugin, client.RegisteredPlugins.First());
            Assert.IsAssignableFrom<AzureStorageAttachment>(registeredPlugin);
        }

        [Fact]
        public void Should_get_back_AzureStorageAttachment_for_container_sas_based_configuration()
        {
            var client = new FakeClientEntity("fake", string.Empty, RetryPolicy.Default);

            var azureStorageAttachmentConfiguration = new AzureStorageAttachmentConfiguration(new StorageCredentials("?sv=2018-03-28&sr=c&sig=5XxlRKoP4yEmibM2HhJlQuV7MG3rYgQXD89mLpNp%2F24%3D"), "http://127.0.0.1:10000/devstoreaccount1");

            var registeredPlugin = AzureStorageAttachmentExtensions.RegisterAzureStorageAttachmentPlugin(client, azureStorageAttachmentConfiguration);

            Assert.Equal(registeredPlugin, client.RegisteredPlugins.First());
            Assert.IsAssignableFrom<AzureStorageAttachment>(registeredPlugin);
        }

        [Fact]
        public void Should_get_back_ReceiveOnlyAzureStorageAttachment_for_receive_only_plugin()
        {
            var client = new FakeClientEntity("fake", string.Empty, RetryPolicy.Default);

            var registeredPlugin = AzureStorageAttachmentExtensions.RegisterAzureStorageAttachmentPluginForReceivingOnly(client, "mySasUriProperty");

            Assert.Equal(registeredPlugin, client.RegisteredPlugins.First());
            Assert.IsAssignableFrom<ReceiveOnlyAzureStorageAttachment>(registeredPlugin);
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