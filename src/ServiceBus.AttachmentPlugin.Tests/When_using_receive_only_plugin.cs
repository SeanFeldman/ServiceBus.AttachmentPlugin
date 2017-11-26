
namespace ServiceBus.AttachmentPlugin.Tests
{
    using System;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Azure.ServiceBus;
    using Xunit;

    public class When_using_receive_only_plugin : IClassFixture<AzureStorageEmulatorFixture>
    {
        [Fact]
        public async Task Should_download_attachment_using_provided_from_sas_uri()
        {
            var payload = "payload";
            var bytes = Encoding.UTF8.GetBytes(payload);
            var message = new Message(bytes)
            {
                MessageId = Guid.NewGuid().ToString(),
            };
            var plugin = new AzureStorageAttachment(new AzureStorageAttachmentConfiguration(
                    connectionString: "UseDevelopmentStorage=true", containerName: "attachments", messagePropertyToIdentifyAttachmentBlob: "attachment-id")
                    .WithSasUri(sasTokenValidationTime: TimeSpan.FromHours(4), messagePropertyToIdentifySasUri: "mySasUriProperty"));
            await plugin.BeforeMessageSend(message);


            var receiveOnlyPlugin = new ReceiveOnlyAzureStorageAttachment("mySasUriProperty");
            var result = await receiveOnlyPlugin.AfterMessageReceive(message);

            Assert.True(message.UserProperties.ContainsKey("mySasUriProperty"));
            Assert.Equal(payload, Encoding.UTF8.GetString(result.Body));
        }
    }
}
