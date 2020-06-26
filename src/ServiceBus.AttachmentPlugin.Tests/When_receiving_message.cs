namespace ServiceBus.AttachmentPlugin.Tests
{
    using System;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Azure.ServiceBus;
    using Xunit;

    public class When_receiving_message : IClassFixture<AzureStorageEmulatorFixture>
    {
        [Fact]
        public async Task Should_throw_exception_with_blob_path_for_blob_that_cant_be_found()
        {
            var payload = "payload";
            var bytes = Encoding.UTF8.GetBytes(payload);
            var message = new Message(bytes)
            {
                MessageId = Guid.NewGuid().ToString(),
            };

            var sendingPlugin = new AzureStorageAttachment(new AzureStorageAttachmentConfiguration(
                connectionString: AzureStorageEmulatorFixture.TestingStorageAccountConnectionString,
                containerName: "attachments"));
            await sendingPlugin.BeforeMessageSend(message);

            var receivingPlugin = new AzureStorageAttachment(new AzureStorageAttachmentConfiguration(
                connectionString: AzureStorageEmulatorFixture.TestingStorageAccountConnectionString,
                containerName: "attachments-wrong-containers"));

            var exception = await Assert.ThrowsAsync<Exception>(() => receivingPlugin.AfterMessageReceive(message));
            Assert.Contains("attachments-wrong-containers", actualString: exception.Message);
            Assert.Contains(message.UserProperties["$attachment.blob"].ToString(), actualString: exception.Message);
        }
    }
}