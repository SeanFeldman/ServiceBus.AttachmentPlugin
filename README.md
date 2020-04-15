<!--
GENERATED FILE - DO NOT EDIT
This file was generated by [MarkdownSnippets](https://github.com/SimonCropp/MarkdownSnippets).
Source File: /README.source.md
To change this file edit the source file and then run MarkdownSnippets.
-->

![Icon](https://github.com/SeanFeldman/ServiceBus.AttachmentPlugin/blob/master/images/project-icon.png)

### This is a plugin for [Microsoft.Azure.ServiceBus client](https://github.com/Azure/azure-service-bus-dotnet/)

Allows sending messages that exceed maximum size by implementing [Claim Check pattern](http://www.enterpriseintegrationpatterns.com/patterns/messaging/StoreInLibrary.html) with Azure Storage.

[![license](https://img.shields.io/github/license/mashape/apistatus.svg)](https://github.com/SeanFeldman/ServiceBus.AttachmentPlugin/blob/master/LICENSE)
[![develop](https://img.shields.io/appveyor/ci/seanfeldman/ServiceBus-AttachmentPlugin/develop.svg?style=flat-square&branch=develop)](https://ci.appveyor.com/project/seanfeldman/ServiceBus-AttachmentPlugin)
[![opened issues](https://img.shields.io/github/issues-raw/badges/shields/website.svg)](https://github.com/SeanFeldman/ServiceBus.AttachmentPlugin/issues)

### Nuget package

[![NuGet Status](https://buildstats.info/nuget/ServiceBus.AttachmentPlugin?includePreReleases=true)](https://www.nuget.org/packages/ServiceBus.AttachmentPlugin/)

Available here http://nuget.org/packages/ServiceBus.AttachmentPlugin

To Install from the Nuget Package Manager Console 

    PM> Install-Package ServiceBus.AttachmentPlugin

<!-- toc -->
## Contents

  * [Examples](#examples)
    * [Convert body into attachment, no matter how big it is](#convert-body-into-attachment-no-matter-how-big-it-is)
    * [Sending a message without exposing the storage account to receivers](#sending-a-message-without-exposing-the-storage-account-to-receivers)
    * [Configure blob container name](#configure-blob-container-name)
    * [Configure message property to identify attachment blob](#configure-message-property-to-identify-attachment-blob)
    * [Configure custom blob name override](#configure-custom-blob-name-override)
    * [Configure message property for SAS uri to attachment blob](#configure-message-property-for-sas-uri-to-attachment-blob)
    * [Configure criteria for message max size identification](#configure-criteria-for-message-max-size-identification)
    * [Configuring connection string provider](#configuring-connection-string-provider)
    * [Configuring plugin using StorageCredentials (Service or Container SAS)](#configuring-plugin-using-storagecredentials-service-or-container-sas)
    * [Using attachments with Azure Functions](#using-attachments-with-azure-functions)
  * [Cleanup](#cleanup)
  * [Who's trusting this plugin in production](#whos-trusting-this-plugin-in-production)
  * [Icon](#icon)<!-- endtoc -->

## Examples

### Convert body into attachment, no matter how big it is

Configuration and registration

<!-- snippet: ConfigurationAndRegistration -->
<a id='snippet-configurationandregistration'/></a>
```cs
var sender = new MessageSender(connectionString, queueName);
var config = new AzureStorageAttachmentConfiguration(storageConnectionString);
sender.RegisterAzureStorageAttachmentPlugin(config);
```
<sup><a href='/src/ServiceBus.AttachmentPlugin.Tests/Snippets.cs#L14-L20' title='File snippet `configurationandregistration` was extracted from'>snippet source</a> | <a href='#snippet-configurationandregistration' title='Navigate to start of snippet `configurationandregistration`'>anchor</a></sup>
<!-- endsnippet -->

Sending

<!-- snippet: AttachmentSending -->
<a id='snippet-attachmentsending'/></a>
```cs
var payload = new MyMessage
{
    MyProperty = "The Value"
};
var serialized = JsonConvert.SerializeObject(payload);
var payloadAsBytes = Encoding.UTF8.GetBytes(serialized);
var message = new Message(payloadAsBytes);
```
<sup><a href='/src/ServiceBus.AttachmentPlugin.Tests/Snippets.cs#L26-L36' title='File snippet `attachmentsending` was extracted from'>snippet source</a> | <a href='#snippet-attachmentsending' title='Navigate to start of snippet `attachmentsending`'>anchor</a></sup>
<!-- endsnippet -->

Receiving

<!-- snippet: AttachmentReceiving -->
<a id='snippet-attachmentreceiving'/></a>
```cs
var receiver = new MessageReceiver(connectionString, entityPath, ReceiveMode.ReceiveAndDelete);
receiver.RegisterAzureStorageAttachmentPlugin(config);
var message = await receiver.ReceiveAsync().ConfigureAwait(false);
// message will contain the original payload
```
<sup><a href='/src/ServiceBus.AttachmentPlugin.Tests/Snippets.cs#L42-L49' title='File snippet `attachmentreceiving` was extracted from'>snippet source</a> | <a href='#snippet-attachmentreceiving' title='Navigate to start of snippet `attachmentreceiving`'>anchor</a></sup>
<!-- endsnippet -->

### Sending a message without exposing the storage account to receivers

Configuration and registration with blob SAS URI

<!-- snippet: ConfigurationAndRegistrationSas -->
<a id='snippet-configurationandregistrationsas'/></a>
```cs
var sender = new MessageSender(connectionString, queueName);
var config = new AzureStorageAttachmentConfiguration(storageConnectionString)
    .WithBlobSasUri(
        sasTokenValidationTime: TimeSpan.FromHours(4),
        messagePropertyToIdentifySasUri: "mySasUriProperty");
sender.RegisterAzureStorageAttachmentPlugin(config);
```
<sup><a href='/src/ServiceBus.AttachmentPlugin.Tests/Snippets.cs#L54-L63' title='File snippet `configurationandregistrationsas` was extracted from'>snippet source</a> | <a href='#snippet-configurationandregistrationsas' title='Navigate to start of snippet `configurationandregistrationsas`'>anchor</a></sup>
<!-- endsnippet -->

Sending

<!-- snippet: AttachmentSendingSas -->
<a id='snippet-attachmentsendingsas'/></a>
```cs
var payload = new MyMessage
{
    MyProperty = "The Value"
};
var serialized = JsonConvert.SerializeObject(payload);
var payloadAsBytes = Encoding.UTF8.GetBytes(serialized);
var message = new Message(payloadAsBytes);
```
<sup><a href='/src/ServiceBus.AttachmentPlugin.Tests/Snippets.cs#L111-L121' title='File snippet `attachmentsendingsas` was extracted from'>snippet source</a> | <a href='#snippet-attachmentsendingsas' title='Navigate to start of snippet `attachmentsendingsas`'>anchor</a></sup>
<!-- endsnippet -->

Receiving only mode (w/o Storage account credentials)

<!-- snippet: AttachmentReceivingSas -->
<a id='snippet-attachmentreceivingsas'/></a>
```cs
// Override message property used to identify SAS URI
// .RegisterAzureStorageAttachmentPluginForReceivingOnly() is using "$attachment.sas.uri" by default
messageReceiver.RegisterAzureStorageAttachmentPluginForReceivingOnly("mySasUriProperty");
var message = await messageReceiver.ReceiveAsync().ConfigureAwait(false);
```
<sup><a href='/src/ServiceBus.AttachmentPlugin.Tests/Snippets.cs#L127-L134' title='File snippet `attachmentreceivingsas` was extracted from'>snippet source</a> | <a href='#snippet-attachmentreceivingsas' title='Navigate to start of snippet `attachmentreceivingsas`'>anchor</a></sup>
<!-- endsnippet -->

### Configure blob container name

Default container name is "attachments". The value is available via `AzureStorageAttachmentConfiguration.DefaultContainerName` constant.

```c#
new AzureStorageAttachmentConfiguration(storageConnectionString, containerName:"blobs");
```

### Configure message property to identify attachment blob

Default blob identifier property name is "$attachment.blob". The value is available via `AzureStorageAttachmentConfiguration.DefaultMessagePropertyToIdentifyAttachmentBlob` constant.

```c#
new AzureStorageAttachmentConfiguration(storageConnectionString, messagePropertyToIdentifyAttachmentBlob: "myblob");
```

### Configure custom blob name override

Default blob name is a GUID.

<!-- snippet: Configure_blob_name_override -->
<a id='snippet-configure_blob_name_override'/></a>
```cs
var sender = new MessageSender(connectionString, queueName);
var config = new AzureStorageAttachmentConfiguration(storageConnectionString)
    .OverrideBlobName(message =>
    {
        var tenantId = message.UserProperties["tenantId"].ToString();
        var blobName = $"{tenantId}/{message.MessageId}";
        return blobName;
    });
sender.RegisterAzureStorageAttachmentPlugin(config);
```
<sup><a href='/src/ServiceBus.AttachmentPlugin.Tests/Snippets.cs#L68-L80' title='File snippet `configure_blob_name_override` was extracted from'>snippet source</a> | <a href='#snippet-configure_blob_name_override' title='Navigate to start of snippet `configure_blob_name_override`'>anchor</a></sup>
<!-- endsnippet -->


### Configure message property for SAS uri to attachment blob

Default SAS uri property name is "$attachment.sas.uri". The value is available via `AzureStorageAttachmentConfigurationExtensions.DefaultMessagePropertyToIdentitySasUri` constant.

Default SAS token validation time is 7 days. The value is available via `AzureStorageAttachmentConfigurationExtensions.DefaultSasTokenValidationTime` constant.

<!-- snippet: Configure_blob_sas_uri_override -->
<a id='snippet-configure_blob_sas_uri_override'/></a>
```cs
new AzureStorageAttachmentConfiguration(storageConnectionString)
    .WithBlobSasUri(
        messagePropertyToIdentifySasUri: "mySasUriProperty",
        sasTokenValidationTime: TimeSpan.FromHours(12));
```
<sup><a href='/src/ServiceBus.AttachmentPlugin.Tests/Snippets.cs#L85-L92' title='File snippet `configure_blob_sas_uri_override` was extracted from'>snippet source</a> | <a href='#snippet-configure_blob_sas_uri_override' title='Navigate to start of snippet `configure_blob_sas_uri_override`'>anchor</a></sup>
<!-- endsnippet -->

### Configure criteria for message max size identification

Default is to convert any body to attachment.

<!-- snippet: Configure_criteria_for_message_max_size_identification -->
<a id='snippet-configure_criteria_for_message_max_size_identification'/></a>
```cs
// messages with body > 200KB should be converted to use attachments
new AzureStorageAttachmentConfiguration(storageConnectionString,
    messageMaxSizeReachedCriteria: message => message.Body.Length > 200 * 1024);
```
<sup><a href='/src/ServiceBus.AttachmentPlugin.Tests/Snippets.cs#L139-L145' title='File snippet `configure_criteria_for_message_max_size_identification` was extracted from'>snippet source</a> | <a href='#snippet-configure_criteria_for_message_max_size_identification' title='Navigate to start of snippet `configure_criteria_for_message_max_size_identification`'>anchor</a></sup>
<!-- endsnippet -->

### Configuring connection string provider

When Storage connection string needs to be retrieved rather than passed in as a plain text, `AzureStorageAttachmentConfiguration` accepts implementation of `IProvideStorageConnectionString`.
The plugin comes with a `PlainTextConnectionStringProvider` and can be used in the following way.

<!-- snippet: Configuring_connection_string_provider -->
<a id='snippet-configuring_connection_string_provider'/></a>
```cs
var provider = new PlainTextConnectionStringProvider(connectionString);
var config = new AzureStorageAttachmentConfiguration(provider);
```
<sup><a href='/src/ServiceBus.AttachmentPlugin.Tests/Snippets.cs#L151-L156' title='File snippet `configuring_connection_string_provider` was extracted from'>snippet source</a> | <a href='#snippet-configuring_connection_string_provider' title='Navigate to start of snippet `configuring_connection_string_provider`'>anchor</a></sup>
<!-- endsnippet -->

### Configuring plugin using StorageCredentials (Service or Container SAS)

<!-- snippet: Configuring_plugin_using_StorageCredentials -->
<a id='snippet-configuring_plugin_using_storagecredentials'/></a>
```cs
var credentials = new StorageCredentials( /*Shared key OR Service SAS OR Container SAS*/);
var config = new AzureStorageAttachmentConfiguration(credentials, blobEndpoint);
```
<sup><a href='/src/ServiceBus.AttachmentPlugin.Tests/Snippets.cs#L162-L167' title='File snippet `configuring_plugin_using_storagecredentials` was extracted from'>snippet source</a> | <a href='#snippet-configuring_plugin_using_storagecredentials' title='Navigate to start of snippet `configuring_plugin_using_storagecredentials`'>anchor</a></sup>
<!-- endsnippet -->

See [`StorageCredentials`](https://docs.microsoft.com/en-us/dotnet/api/microsoft.azure.storage.auth.storagecredentials) for more details.

### Using attachments with Azure Functions

Azure Functions currently has no way to register plugins, these extension methods are a workaround until this feature is added. 

To use the extensions, your Function must return (send) or take as parameter (receive) an instance of [`Message`](https://docs.microsoft.com/en-us/dotnet/api/microsoft.azure.servicebus.message).

Upload attachment to Azure Storage blob

<!-- snippet: Upload_attachment_without_registering_plugin -->
<a id='snippet-upload_attachment_without_registering_plugin'/></a>
```cs
//To make it possible to use SAS URI when downloading, use WithBlobSasUri() when creating configuration object
await message.UploadAzureStorageAttachment(config);
```
<sup><a href='/src/ServiceBus.AttachmentPlugin.Tests/Snippets.cs#L172-L177' title='File snippet `upload_attachment_without_registering_plugin` was extracted from'>snippet source</a> | <a href='#snippet-upload_attachment_without_registering_plugin' title='Navigate to start of snippet `upload_attachment_without_registering_plugin`'>anchor</a></sup>
<!-- endsnippet -->

Download attachment from Azure Storage blob

<!-- snippet: Download_attachment_without_registering_plugin -->
<a id='snippet-download_attachment_without_registering_plugin'/></a>
```cs
//Using SAS URI with default message property ($attachment.sas.uri)
await message.DownloadAzureStorageAttachment();

//Using SAS URI with custom message property
await message.DownloadAzureStorageAttachment("$custom-attachment.sas.uri");

//Using configuration object
await message.DownloadAzureStorageAttachment(config);
```
<sup><a href='/src/ServiceBus.AttachmentPlugin.Tests/Snippets.cs#L181-L192' title='File snippet `download_attachment_without_registering_plugin` was extracted from'>snippet source</a> | <a href='#snippet-download_attachment_without_registering_plugin' title='Navigate to start of snippet `download_attachment_without_registering_plugin`'>anchor</a></sup>
<!-- endsnippet -->

#### Additional providers

* [ServiceBus.AttachmentPlugin.KeyVaultProvider](https://www.nuget.org/packages?q=ServiceBus.AttachmentPlugin.KeyVaultProvider)

## Cleanup

The plugin does **NOT** implement cleanup for the reasons stated [here](https://github.com/SeanFeldman/ServiceBus.AttachmentPlugin/issues/86#issuecomment-458541694). When cleanup is required, there are a few [options available](https://github.com/SeanFeldman/ServiceBus.AttachmentPlugin/issues/86#issue-404101630) depending on the use case.

## Who's trusting this plugin in production

![Microsoft](https://github.com/SeanFeldman/ServiceBus.AttachmentPlugin/blob/develop/images/using/microsoft.png)
![Codit](https://github.com/SeanFeldman/ServiceBus.AttachmentPlugin/blob/develop/images/using/Codit.png)
![Hemonto](https://github.com/SeanFeldman/ServiceBus.AttachmentPlugin/blob/develop/images/using/Hemonto.png)

Proudly list your company here if use this plugin in production

## Icon

Created by Dinosoft Labs from the Noun Project.
