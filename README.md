![Icon](https://github.com/SeanFeldman/ServiceBus.AttachmentPlugin/blob/master/images/project-icon.png)

### This is an add-in for [Microsoft.Azure.ServiceBus client](https://github.com/Azure/azure-service-bus-dotnet/) 

Allows sending messages that exceed maxinum size by implementing [Claim Check pattern](http://www.enterpriseintegrationpatterns.com/patterns/messaging/StoreInLibrary.html) with Azure Storage.

### Nuget package [![NuGet Status](https://buildstats.info/nuget/ServiceBus.AttachmentPlugin?includePreReleases=true)](https://www.nuget.org/packages/ServiceBus.AttachmentPlugin/) [![Build Status](https://img.shields.io/appveyor/ci/seanfeldman/ServiceBus-AttachmentPlugin/master.svg?style=flat-square)](https://ci.appveyor.com/project/seanfeldman/ServiceBus-AttachmentPlugin) [![License](https://img.shields.io/github/license/mashape/apistatus.svg)](https://github.com/SeanFeldman/ServiceBus.AttachmentPlugin/blob/master/LICENSE) [![Issues](https://img.shields.io/github/issues-raw/badges/shields/website.svg)](https://github.com/SeanFeldman/ServiceBus.AttachmentPlugin)

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

Configuration and registration with SAS uri

```c#
var sender = new MessageSender(connectionString, queueName);
var config = new AzureStorageAttachmentConfiguration(storageConnectionString)
	.WithSasUri(sasTokenValidationTime: TimeSpan.FromHours(4), messagePropertyToIdentifySasUri: "mySasUriProperty");
sender.RegisterAzureStorageAttachmentPlugin(config);
```  

Sending

```c#
var payload = new MyMessage { ... }; 
var serialized = JsonConvert.SerializeObject(payload);
var payloadAsBytes = Encoding.UTF8.GetBytes(serialized);
var message = new Message(payloadAsBytes);
```

Receive

```c#
var receiver = new MessageReceiver(connectionString, entityPath, ReceiveMode.ReceiveAndDelete);
var msg = await receiver.ReceiveAsync().ConfigureAwait(false);
// msg will contain the original payload
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

## Icon

Created by Dinosoft Labs from the Noun Project.