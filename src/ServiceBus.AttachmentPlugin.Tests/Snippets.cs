using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.WindowsAzure.Storage.Auth;
using Newtonsoft.Json;

class Snippets
{
    void ConfigurationAndRegistration(string connectionString, string queueName, string storageConnectionString)
    {
        #region ConfigurationAndRegistration

        var sender = new MessageSender(connectionString, queueName);
        var config = new AzureStorageAttachmentConfiguration(storageConnectionString);
        sender.RegisterAzureStorageAttachmentPlugin(config);

        #endregion
    }

    void AttachmentSending()
    {
        #region AttachmentSending

        var payload = new MyMessage
        {
            MyProperty = "The Value"
        };
        var serialized = JsonConvert.SerializeObject(payload);
        var payloadAsBytes = Encoding.UTF8.GetBytes(serialized);
        var message = new Message(payloadAsBytes);

        #endregion
    }

    async Task AttachmentReceiving(string connectionString, string entityPath, AzureStorageAttachmentConfiguration config)
    {
        #region AttachmentReceiving

        var receiver = new MessageReceiver(connectionString, entityPath, ReceiveMode.ReceiveAndDelete);
        receiver.RegisterAzureStorageAttachmentPlugin(config);
        var msg = await receiver.ReceiveAsync().ConfigureAwait(false);
        // msg will contain the original payload

        #endregion
    }

    void ConfigurationAndRegistrationSas(string connectionString, string queueName, string storageConnectionString)
    {
        #region ConfigurationAndRegistrationSas

        var sender = new MessageSender(connectionString, queueName);
        var config = new AzureStorageAttachmentConfiguration(storageConnectionString)
            .WithBlobSasUri(
                sasTokenValidationTime: TimeSpan.FromHours(4),
                messagePropertyToIdentifySasUri: "mySasUriProperty");
        sender.RegisterAzureStorageAttachmentPlugin(config);

        #endregion
    }

    void AttachmentSendingSas()
    {
        #region AttachmentSendingSas

        var payload = new MyMessage
        {
            MyProperty = "The Value"
        };
        var serialized = JsonConvert.SerializeObject(payload);
        var payloadAsBytes = Encoding.UTF8.GetBytes(serialized);
        var message = new Message(payloadAsBytes);

        #endregion
    }

    async Task AttachmentReceivingSas(MessageReceiver messageReceiver)
    {
        #region AttachmentReceivingSas

        // Override message property used to identify SAS URI
        // .RegisterAzureStorageAttachmentPluginForReceivingOnly() is using "$attachment.sas.uri" by default
        messageReceiver.RegisterAzureStorageAttachmentPluginForReceivingOnly("mySasUriProperty");
        var message = await messageReceiver.ReceiveAsync().ConfigureAwait(false);

        #endregion
    }

    void Configure_criteria_for_message_max_size_identification(string storageConnectionString)
    {
        #region Configure_criteria_for_message_max_size_identification

        // messages with body > 200KB should be converted to use attachments
        new AzureStorageAttachmentConfiguration(storageConnectionString,
            messageMaxSizeReachedCriteria: message => message.Body.Length > 200 * 1024);

        #endregion
    }

    void Configuring_connection_string_provider(string connectionString)
    {
        #region Configuring_connection_string_provider

        var provider = new PlainTextConnectionStringProvider(connectionString);
        var config = new AzureStorageAttachmentConfiguration(provider);

        #endregion
    }

    void Configuring_plugin_using_StorageCredentials(string connectionString, string blobEndpoint)
    {
        #region Configuring_plugin_using_StorageCredentials

        var credentials = new StorageCredentials( /*Shared key OR Service SAS OR Container SAS*/);
        var config = new AzureStorageAttachmentConfiguration(credentials, blobEndpoint);

        #endregion
    }
}

class MyMessage
{
    public string MyProperty { get; set; }
}