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

toc

## Examples

### Convert body into attachment, no matter how big it is

Configuration and registration

snippet: ConfigurationAndRegistration

Sending

snippet: AttachmentSending

Receiving

snippet: AttachmentReceiving

### Sending a message without exposing the storage account to receivers

Configuration and registration with blob SAS URI

snippet: ConfigurationAndRegistrationSas

Sending

snippet: AttachmentSendingSas

Receiving only mode (w/o Storage account credentials)

snippet: AttachmentReceivingSas

### Configure blob container name

Default container name is "attachments".

```c#
new AzureStorageAttachmentConfiguration(storageConnectionString, containerName:"blobs");
```

### Configure message property to identify attachment blob

Default blob identifier property name is "$attachment.blob".

```c#
new AzureStorageAttachmentConfiguration(storageConnectionString, messagePropertyToIdentifyAttachmentBlob: "myblob");
```

### Configure message property for SAS uri to attachment blob

Default SAS uri property name is "$attachment.sas.uri".

```c#
new AzureStorageAttachmentConfiguration(storageConnectionString).WithSasUri(messagePropertyToIdentifySasUri: "mySasUriProperty");
```

### Configure criteria for message max size identification

Default is to convert any body to attachment.

snippet: Configure_criteria_for_message_max_size_identification

### Configuring connection string provider

When Storage connection string needs to be retrieved rather than passed in as a plain text, `AzureStorageAttachmentConfiguration` accepts implementation of `IProvideStorageConnectionString`.
The plugin comes with a `PlainTextConnectionStringProvider` and can be used in the following way.

snippet: Configuring_connection_string_provider

### Configuring plugin using StorageCredentials (Service or Container SAS)

snippet: Configuring_plugin_using_StorageCredentials

See [`StorageCredentials`](https://docs.microsoft.com/en-us/dotnet/api/microsoft.windowsazure.storage.auth.storagecredentials) for more details.

### Using attachments with Azure Functions

Azure Functions currently has no way to register plugins, these extension methods are a workaround until this feature is added. 

To use the extensions, your Function must return (send) or take as parameter (receive) an instance of [`Message`](https://docs.microsoft.com/en-us/dotnet/api/microsoft.azure.servicebus.message).

Upload attachment to Azure Storage blob

snippet: Upload_attachment_without_registering_plugin

Download attachment from Azure Storage blob

snippet: Download_attachment_without_registering_plugin

#### Additional providers

* [ServiceBus.AttachmentPlugin.KeyVaultProvider](https://www.nuget.org/packages?q=ServiceBus.AttachmentPlugin.KeyVaultProvider)

## Cleanup

The plugin does **NOT** implement cleanup for the reasons stated [here](https://github.com/SeanFeldman/ServiceBus.AttachmentPlugin/issues/86#issuecomment-458541694). When cleanup is required, there are a few [options available](https://github.com/SeanFeldman/ServiceBus.AttachmentPlugin/issues/86#issue-404101630) depending on the use case.

## Who's trusting this plugin in production

![Microsoft](https://github.com/SeanFeldman/ServiceBus.AttachmentPlugin/blob/develop/images/using/microsoft.png)
![Codit](https://github.com/SeanFeldman/ServiceBus.AttachmentPlugin/blob/master/images/using/Codit.png)

Proudly list your company here if use this plugin in production

## Icon

Created by Dinosoft Labs from the Noun Project.
