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

## Examples

### Convert body into attachment, no matter how big it is

Configuration and registration

```c#
var sender = new MessageSender(connectionString, queueName);
var config = new AzureStorageAttachmentConfiguration(storageConnectionString);
sender.RegisterAzureStorageAttachmentPlugin(config);
```        

Sending

```c#
var payload = new MyMessage { ... }; 
var serialized = JsonConvert.SerializeObject(payload);
var payloadAsBytes = Encoding.UTF8.GetBytes(serialized);
var message = new Message(payloadAsBytes);
```


Receiving

```c#
var receiver = new MessageReceiver(connectionString, entityPath, ReceiveMode.ReceiveAndDelete);
receiver.RegisterAzureStorageAttachmentPlugin(config);
var msg = await receiver.ReceiveAsync().ConfigureAwait(false);
// msg will contain the original payload
```

### Sending a message without exposing the storage account to receivers

Configuration and registration with blob SAS URI

```c#
var sender = new MessageSender(connectionString, queueName);
var config = new AzureStorageAttachmentConfiguration(storageConnectionString)
	.WithBlobSasUri(sasTokenValidationTime: TimeSpan.FromHours(4), messagePropertyToIdentifySasUri: "mySasUriProperty");
sender.RegisterAzureStorageAttachmentPlugin(config);
```  

Sending

```c#
var payload = new MyMessage { ... }; 
var serialized = JsonConvert.SerializeObject(payload);
var payloadAsBytes = Encoding.UTF8.GetBytes(serialized);
var message = new Message(payloadAsBytes);
```

Receiving only mode (w/o Storage account credentials)

```c#
// Overide message property used to identify SAS URI
// .RegisterAzureStorageAttachmentPluginForReceivingOnly() is using "$attachment.sas.uri" by default
var receiver = messageReceiver.RegisterAzureStorageAttachmentPluginForReceivingOnly("mySasUriProperty");
var msg = await receiver.ReceiveAsync().ConfigureAwait(false);
```


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

```c#
// messages with body > 200KB should be converted to use attachments
new AzureStorageAttachmentConfiguration(storageConnectionString, message => message.Body.Length > 200 * 1024);
```

### Configuring connection string provider

When Storage connection string needs to be retrieved rather than passed in as a plain text, `AzureStorageAttachmentConfiguration` accepts implementation of `IProvideStorageConnectionString`.
The plugin comes with a `PlainTextConnectionStringProvider` and can be used in the following way.

```c#
var provider = new PlainTextConnectionStringProvider("connectionString");
var config = new AzureStorageAttachmentConfiguration(provider);
```

### Configuring plugin using StorageCredentials (Service or Container SAS)

```c#
var credentials = new StorageCredentials(/*Shared key OR Service SAS OR Container SAS*/);
var config = new AzureStorageAttachmentConfiguration(credentials);

See [`StorageCredentials`](https://docs.microsoft.com/en-us/dotnet/api/microsoft.windowsazure.storage.auth.storagecredentials) for more details.
```


#### Additional providers

* [ServiceBus.AttachmentPlugin.KeyVaultProvider](https://www.nuget.org/packages?q=ServiceBus.AttachmentPlugin.KeyVaultProvider)

## Who's trusting this plugin in production

![Microsoft](https://github.com/SeanFeldman/ServiceBus.AttachmentPlugin/blob/develop/images/using/microsoft.png)
![Codit](https://github.com/SeanFeldman/ServiceBus.AttachmentPlugin/blob/master/images/using/Codit.png)

Proudly list your company here if use this plugin in production

## Icon

Created by Dinosoft Labs from the Noun Project.
