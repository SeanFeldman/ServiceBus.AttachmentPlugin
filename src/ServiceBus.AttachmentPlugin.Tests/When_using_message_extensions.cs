namespace ServiceBus.AttachmentPlugin.Tests
{
    using System;
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
                connectionString: AzureStorageEmulatorFixture.TestingStorageAccountConnectionString,
                messagePropertyToIdentifyAttachmentBlob: "attachment-id");

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
                    connectionString: AzureStorageEmulatorFixture.TestingStorageAccountConnectionString,
                    messagePropertyToIdentifyAttachmentBlob: "attachment-id")
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
                connectionString: AzureStorageEmulatorFixture.TestingStorageAccountConnectionString,
                messagePropertyToIdentifyAttachmentBlob: "attachment-id")
            .WithBlobSasUri(customSasUri);

            await message.UploadAzureStorageAttachment(configuration);

            Assert.Null(message.Body);

            var receivedMessage = await message.DownloadAzureStorageAttachment(customSasUri);

            Assert.Equal(payload, Encoding.UTF8.GetString(receivedMessage.Body));
        }

        [Fact]
        public async Task Should_be_able_to_override_blob_name_and_receive_message_payload_using_the_new_name()
        {
            var payload = "payload";
            var bytes = Encoding.UTF8.GetBytes(payload);
            var message = new Message(bytes) { MessageId = Guid.NewGuid().ToString("N") };
            var configuration = new AzureStorageAttachmentConfiguration(
                connectionString: AzureStorageEmulatorFixture.TestingStorageAccountConnectionString,
                messagePropertyToIdentifyAttachmentBlob: "attachment-id");

            configuration.OverrideBlobName(msg => $"test/{msg.MessageId}");

            await message.UploadAzureStorageAttachment(configuration);

            Assert.Null(message.Body);

            var receivedMessage = await message.DownloadAzureStorageAttachment(configuration);

            Assert.Equal(payload, Encoding.UTF8.GetString(receivedMessage.Body));
            Assert.Equal($"test/{message.MessageId}", message.UserProperties[configuration.MessagePropertyToIdentifyAttachmentBlob]);
        }

        [Fact]
        public async Task Should_set_body_to_the_value_provided_by_body_replacer_override()
        {
            var payload = "payload";
            var bytes = Encoding.UTF8.GetBytes(payload);
            var message = new Message(bytes)
            {
                MessageId = Guid.NewGuid().ToString(),
            };
            var plugin = new AzureStorageAttachment(new AzureStorageAttachmentConfiguration(
                    connectionString: AzureStorageEmulatorFixture.TestingStorageAccountConnectionString,
                    messagePropertyToIdentifyAttachmentBlob: "attachment-id")
                .OverrideBody(msg => Array.Empty<byte>()));
            var result = await plugin.BeforeMessageSend(message);

            Assert.NotNull(result.Body);
            Assert.Equal(Array.Empty<byte>(), result.Body);
        }

        [Fact]
        public async Task Should_set_body_to_null_if_body_replacer_override_is_not_provided()
        {
            var payload = "payload";
            var bytes = Encoding.UTF8.GetBytes(payload);
            var message = new Message(bytes)
            {
                MessageId = Guid.NewGuid().ToString(),
            };
            var plugin = new AzureStorageAttachment(new AzureStorageAttachmentConfiguration(
                AzureStorageEmulatorFixture.TestingStorageAccountConnectionString,
                messagePropertyToIdentifyAttachmentBlob: "attachment-id"));
            var result = await plugin.BeforeMessageSend(message);

            Assert.Null(result.Body);
        }
    }
}