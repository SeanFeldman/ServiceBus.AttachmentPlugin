namespace ServiceBus.AttachmentPlugin.Tests
{
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Azure.ServiceBus;
    using Xunit;

    public class When_using_message_extensions : IClassFixture<AzureStorageEmulatorFixture>
    {
        // ReSharper disable once NotAccessedField.Local
        readonly AzureStorageEmulatorFixture fixture;

        public When_using_message_extensions(AzureStorageEmulatorFixture fixture)
        {
            this.fixture = fixture;
        }

        [Fact]
        public async Task Should_send_and_receive_with_configuration()
        {
            var payload = "payload";
            var bytes = Encoding.UTF8.GetBytes(payload);
            var message = new Message(bytes);
            var configuration = new AzureStorageAttachmentConfiguration(
                connectionStringProvider: AzureStorageEmulatorFixture.ConnectionStringProvider, containerName: "attachments", messagePropertyToIdentifyAttachmentBlob: "attachment-id");

            await message.UploadAzureStorageAttachment(configuration);

            Assert.Null(message.Body);

            var receivedMessage = await message.DownloadAzureStorageAttachment(configuration);

            Assert.Equal(payload, Encoding.UTF8.GetString(receivedMessage.Body));
        }

        [Fact]
        public async Task Should_send_and_receive_with_default_sas_uri_property()
        {
            var payload = "payload";
            var bytes = Encoding.UTF8.GetBytes(payload);
            var message = new Message(bytes);
            var configuration = new AzureStorageAttachmentConfiguration(
                connectionStringProvider: AzureStorageEmulatorFixture.ConnectionStringProvider, containerName: "attachments", messagePropertyToIdentifyAttachmentBlob: "attachment-id")
            .WithBlobSasUri();

            await message.UploadAzureStorageAttachment(configuration);

            Assert.Null(message.Body);

            var receivedMessage = await message.DownloadAzureStorageAttachment();

            Assert.Equal(payload, Encoding.UTF8.GetString(receivedMessage.Body));
        }

        [Fact]
        public async Task Should_send_and_receive_with_custom_sas_uri_property()
        {
            var payload = "payload";
            var bytes = Encoding.UTF8.GetBytes(payload);
            var message = new Message(bytes);
            var customSasUri = "$custom-attachment.sas.uri";
            var configuration = new AzureStorageAttachmentConfiguration(
                connectionStringProvider: AzureStorageEmulatorFixture.ConnectionStringProvider, containerName: "attachments", messagePropertyToIdentifyAttachmentBlob: "attachment-id")
            .WithBlobSasUri(customSasUri);

            await message.UploadAzureStorageAttachment(configuration);

            Assert.Null(message.Body);

            var receivedMessage = await message.DownloadAzureStorageAttachment(customSasUri);

            Assert.Equal(payload, Encoding.UTF8.GetString(receivedMessage.Body));
        }
    }
}