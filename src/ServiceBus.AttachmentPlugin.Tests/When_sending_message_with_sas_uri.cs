using System;
using System.Text;

namespace ServiceBus.AttachmentPlugin.Tests
{
    using System.Threading.Tasks;
    using Microsoft.Azure.ServiceBus;
    using Xunit;

    public class When_sending_message_with_sas_uri : IClassFixture<AzureStorageEmulatorFixture>
    {

        [Fact]
        public async Task Should_set_sas_uri_when_specified()
        {
            var payload = "payload";
            var bytes = Encoding.UTF8.GetBytes(payload);
            var message = new Message(bytes)
            {
                MessageId = Guid.NewGuid().ToString(),
            };
            var plugin = new AzureStorageAttachment(new AzureStorageAttachmentConfiguration(
                connectionString: "UseDevelopmentStorage=true", containerName: "attachments", messagePropertyToIdentifyAttachmentBlob: "attachment-id", sasTokenValidInSeconds: 60*60*24, sasUri: "mySasUriProperty"));
            var result = await plugin.BeforeMessageSend(message);

            Assert.Null(result.Body);
            Assert.True(message.UserProperties.ContainsKey("attachment-id"));
            Assert.True(message.UserProperties.ContainsKey("mySasUriProperty"));
        }
    }
}
