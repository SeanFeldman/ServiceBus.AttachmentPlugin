namespace ServiceBus.AttachmentPlugin.Tests
{
    using System;
    using System.Text;
    using System.Threading.Tasks;
    using Azure.Storage.Blobs;
    using Azure.Storage.Blobs.Models;
    using Microsoft.Azure.ServiceBus;
    using Xunit;

    public class When_sending_message_using_connection_string : IClassFixture<AzureStorageEmulatorFixture>
    {
        readonly AzureStorageEmulatorFixture fixture;

        public When_sending_message_using_connection_string(AzureStorageEmulatorFixture fixture)
        {
            this.fixture = fixture;
        }

        [Fact]
        public async Task Should_nullify_body_when_body_should_be_sent_as_attachment()
        {
            var payload = "payload";
            var bytes = Encoding.UTF8.GetBytes(payload);
            var message = new Message(bytes)
            {
                MessageId = Guid.NewGuid().ToString(),
            };
            var plugin = new AzureStorageAttachment(new AzureStorageAttachmentConfiguration(
                connectionString: AzureStorageEmulatorFixture.TestingStorageAccountConnectionString,
                messagePropertyToIdentifyAttachmentBlob:"attachment-id"));
            var result = await plugin.BeforeMessageSend(message);

            Assert.Null(result.Body);
            Assert.True(message.UserProperties.ContainsKey("attachment-id"));
        }

        [Fact]
        public async Task Should_leave_body_as_is_for_message_not_exceeding_max_size()
        {
            var payload = "payload";
            var bytes = Encoding.UTF8.GetBytes(payload);
            var message = new Message(bytes)
            {
                MessageId = Guid.NewGuid().ToString(),
                TimeToLive = TimeSpan.FromHours(1)
            };
            var plugin = new AzureStorageAttachment(new AzureStorageAttachmentConfiguration(
                connectionString: AzureStorageEmulatorFixture.TestingStorageAccountConnectionString, 
                messagePropertyToIdentifyAttachmentBlob:"attachment-id",
                messageMaxSizeReachedCriteria:msg => msg.Body.Length > 100));
            var result = await plugin.BeforeMessageSend(message);

            Assert.NotNull(result.Body);
            Assert.False(message.UserProperties.ContainsKey("attachment-id"));
        }

        [Fact]
        public async Task Should_set_valid_until_datetime_on_blob_same_as_message_TTL()
        {
            var payload = "payload";
            var bytes = Encoding.UTF8.GetBytes(payload);
            var message = new Message(bytes)
            {
                MessageId = Guid.NewGuid().ToString(),
                TimeToLive = TimeSpan.FromHours(1)
            };
            var dateTimeNowUtc = new DateTime(2017, 1, 2);
            var configuration = new AzureStorageAttachmentConfiguration(
                connectionString: AzureStorageEmulatorFixture.TestingStorageAccountConnectionString,
                messagePropertyToIdentifyAttachmentBlob:"attachment-id");
            AzureStorageAttachment.DateTimeFunc = () => dateTimeNowUtc;
            var plugin = new AzureStorageAttachment(configuration);
            await plugin.BeforeMessageSend(message);

            var container = configuration.ContainerName;
            var blobName = (string)message.UserProperties[configuration.MessagePropertyToIdentifyAttachmentBlob];
            var blob = new BlobClient(connectionString: AzureStorageEmulatorFixture.TestingStorageAccountConnectionString, blobContainerName: container, blobName: blobName);
            BlobProperties properties = await blob.GetPropertiesAsync();
            var validUntil = properties.Metadata[AzureStorageAttachment.ValidUntilUtc];
            Assert.Equal(dateTimeNowUtc.Add(message.TimeToLive).ToString(AzureStorageAttachment.DateFormat), validUntil);
        }

        [Fact]
        public async Task Should_receive_it_using_connection_string()
        {
            var payload = "payload";
            var bytes = Encoding.UTF8.GetBytes(payload);
            var message = new Message(bytes);
            var configuration = new AzureStorageAttachmentConfiguration(
                connectionString: AzureStorageEmulatorFixture.TestingStorageAccountConnectionString,
                messagePropertyToIdentifyAttachmentBlob: "attachment-id");

            var plugin = new AzureStorageAttachment(configuration);
            await plugin.BeforeMessageSend(message);

            Assert.Null(message.Body);

            var receivedMessage = await plugin.AfterMessageReceive(message);

            Assert.Equal(payload, Encoding.UTF8.GetString(receivedMessage.Body));
        }

        [Fact]
        public async Task Should_not_reupload_blob_if_one_is_already_assigned()
        {
            var payload = "payload";
            var bytes = Encoding.UTF8.GetBytes(payload);
            var message = new Message(bytes);
            var configuration = new AzureStorageAttachmentConfiguration(
                connectionString: AzureStorageEmulatorFixture.TestingStorageAccountConnectionString,
                messagePropertyToIdentifyAttachmentBlob: "attachment-id");

            var plugin = new AzureStorageAttachment(configuration);

            var processedMessage = await plugin.BeforeMessageSend(message);

            var blobId = processedMessage.UserProperties["attachment-id"];

            var reprocessedMessage = await plugin.BeforeMessageSend(message);

            Assert.Equal(blobId, reprocessedMessage.UserProperties["attachment-id"]);
        }

        [Fact]
        public async Task Should_not_set_sas_uri_by_default()
        {
            var payload = "payload";
            var bytes = Encoding.UTF8.GetBytes(payload);
            var message = new Message(bytes)
            {
                MessageId = Guid.NewGuid().ToString(),
            };
            var plugin = new AzureStorageAttachment(new AzureStorageAttachmentConfiguration(
                connectionString: AzureStorageEmulatorFixture.TestingStorageAccountConnectionString,
                messagePropertyToIdentifyAttachmentBlob: "attachment-id"));
            var result = await plugin.BeforeMessageSend(message);

            Assert.Null(result.Body);
            Assert.True(message.UserProperties.ContainsKey("attachment-id"));
            Assert.False(message.UserProperties.ContainsKey("$attachment.sas.uri"));
        }

        [Fact]
        public async Task Should_be_able_to_receive_using_container_sas()
        {
            var payload = "payload";
            var bytes = Encoding.UTF8.GetBytes(payload);
            var message = new Message(bytes);
            var configuration = new AzureStorageAttachmentConfiguration(
                connectionString: AzureStorageEmulatorFixture.TestingStorageAccountConnectionString,
                messagePropertyToIdentifyAttachmentBlob: "attachment-id");

            var plugin = new AzureStorageAttachment(configuration);
            await plugin.BeforeMessageSend(message);

            Assert.Null(message.Body);

            var credentials = await AzureStorageEmulatorFixture.GetContainerSas("attachments");
            var receiveConfiguration = new AzureStorageAttachmentConfiguration(credentials, messagePropertyToIdentifyAttachmentBlob: "attachment-id");

            var receivePlugin = new AzureStorageAttachment(receiveConfiguration);

            var receivedMessage = await receivePlugin.AfterMessageReceive(message);

            Assert.Equal(payload, Encoding.UTF8.GetString(receivedMessage.Body));
        }

        [Fact]
        public async Task Should_be_able_to_send_if_container_was_not_found()
        {
            await fixture.DeleteContainer("attachments-that-didnt-exist");

            var payload = "payload";
            var bytes = Encoding.UTF8.GetBytes(payload);
            var message = new Message(bytes);
            var configuration = new AzureStorageAttachmentConfiguration(
                connectionString: AzureStorageEmulatorFixture.TestingStorageAccountConnectionString,
                containerName: "attachments-that-didnt-exist",
                messagePropertyToIdentifyAttachmentBlob: "attachment-id");

            var plugin = new AzureStorageAttachment(configuration);
            await plugin.BeforeMessageSend(message);

            Assert.Null(message.Body);

            var receivedMessage = await plugin.AfterMessageReceive(message);

            Assert.Equal(payload, Encoding.UTF8.GetString(receivedMessage.Body));
        }
    }
}