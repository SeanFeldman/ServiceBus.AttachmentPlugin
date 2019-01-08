namespace ServiceBus.AttachmentPlugin.Tests
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Net;
    using System.Net.NetworkInformation;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.ServiceBus;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;

    public class AzureStorageEmulatorFixture
    {
        public static IProvideStorageConnectionString ConnectionStringProvider = new PlainTextConnectionStringProvider("UseDevelopmentStorage=true");

        public AzureStorageEmulatorFixture()
        {
            AzureStorageEmulatorManager.StartStorageEmulator();

            // Emulator is not started fast enough on AppVeyor
            // Microsoft.WindowsAzure.Storage.StorageException : Unable to connect to the remote server

            var properties = IPGlobalProperties.GetIPGlobalProperties();
            bool emulatorStarted;

            var stopwatch = Stopwatch.StartNew();

            do
            {
                var endpoints = properties.GetActiveTcpListeners();
                emulatorStarted = endpoints.Any(x => x.Port == 10000 && Equals(x.Address, IPAddress.Loopback));
                Console.WriteLine("waiting for emulator to start...");
                Thread.Sleep(100);
            } while (emulatorStarted == false && stopwatch.Elapsed < TimeSpan.FromSeconds(60));

            if (emulatorStarted == false)
            {
                throw new Exception("Storage emulator failed to start after 60 seconds.");
            }
        }

        public string GetBlobEndpoint()
        {
            return CloudStorageAccount.DevelopmentStorageAccount.BlobEndpoint.ToString();
        }

        public async Task<string> GetContainerSas(string containerName)
        {
            // get container
            var blobClient = CloudStorageAccount.DevelopmentStorageAccount.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference(containerName);
            if (!await container.ExistsAsync())
            {
                await container.CreateIfNotExistsAsync();
            }

            await container.FetchAttributesAsync();

            // create access policy and store it
            var accessPolicyId = "test-policy";

            var permissions = new BlobContainerPermissions
            {
                PublicAccess = BlobContainerPublicAccessType.Off,
                SharedAccessPolicies = {
                {
                    accessPolicyId,
                    new SharedAccessBlobPolicy
                    {
                        Permissions = SharedAccessBlobPermissions.Add | SharedAccessBlobPermissions.Create | SharedAccessBlobPermissions.Read | SharedAccessBlobPermissions.Write,
                        SharedAccessStartTime = DateTimeOffset.UtcNow.Subtract(TimeSpan.FromMinutes(1)),
                        SharedAccessExpiryTime = DateTimeOffset.UtcNow.AddDays(1)
                    }
                }}
            };

            await container.SetPermissionsAsync(permissions);

            // create SAS with policy
            return container.GetSharedAccessSignature(null, accessPolicyId);
        }

        public async Task CreateContainer(string containerName)
        {
            var containerUri = new Uri($"{CloudStorageAccount.DevelopmentStorageAccount.BlobEndpoint}/{containerName}");
            var container = new CloudBlobContainer(containerUri, CloudStorageAccount.DevelopmentStorageAccount.Credentials);
            if (!await container.ExistsAsync())
            {
                await container.CreateIfNotExistsAsync();
            }
        }
        public async Task DeleteContainer(string containerName)
        {
            var containerUri = new Uri($"{CloudStorageAccount.DevelopmentStorageAccount.BlobEndpoint}/{containerName}");
            var container = new CloudBlobContainer(containerUri, CloudStorageAccount.DevelopmentStorageAccount.Credentials);
            if (await container.ExistsAsync())
            {
                await container.DeleteIfExistsAsync();
            }
        }
    }
}