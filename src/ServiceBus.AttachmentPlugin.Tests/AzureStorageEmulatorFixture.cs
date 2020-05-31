namespace ServiceBus.AttachmentPlugin.Tests
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Azure.ServiceBus;
    using Microsoft.Azure.Storage;
    using Microsoft.Azure.Storage.Blob;

    public class AzureStorageEmulatorFixture
    {
        public static IProvideStorageConnectionString ConnectionStringProvider = new PlainTextConnectionStringProvider("UseDevelopmentStorage=true");
        CloudStorageAccount DevelopmentStorageAccount = CloudStorageAccount.DevelopmentStorageAccount;

        public string GetBlobEndpoint()
        {
            return DevelopmentStorageAccount.BlobEndpoint.ToString();
        }

        public async Task<string> GetContainerSas(string containerName)
        {
            // get container
            var blobClient = DevelopmentStorageAccount.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference(containerName);
            if (!await container.ExistsAsync())
            {
                await container.CreateIfNotExistsAsync();
            }

            await container.FetchAttributesAsync();
            var permissionsFound = await container.GetPermissionsAsync();

            // create access policy and store it
            var accessPolicyId = "test-policy";

            if (!permissionsFound.SharedAccessPolicies.ContainsKey(accessPolicyId))
            {
                var permissions = new BlobContainerPermissions
                {
                    PublicAccess = BlobContainerPublicAccessType.Off,
                    SharedAccessPolicies =
                    {
                        {
                            accessPolicyId,
                            new SharedAccessBlobPolicy
                            {
                                Permissions = SharedAccessBlobPermissions.Add | SharedAccessBlobPermissions.Create | SharedAccessBlobPermissions.Read | SharedAccessBlobPermissions.Write,
                                SharedAccessStartTime = DateTimeOffset.UtcNow.Subtract(TimeSpan.FromMinutes(1)),
                                SharedAccessExpiryTime = DateTimeOffset.UtcNow.AddDays(1)
                            }
                        }
                    }
                };

                await container.SetPermissionsAsync(permissions);
            }
            else
            {
                permissionsFound.SharedAccessPolicies[accessPolicyId].SharedAccessExpiryTime = DateTimeOffset.UtcNow.AddDays(1);
                await container.SetPermissionsAsync(permissionsFound);
            }

            // create SAS with policy
            return container.GetSharedAccessSignature(null, accessPolicyId);
        }

        public async Task CreateContainer(string containerName)
        {
            var containerUri = new Uri($"{DevelopmentStorageAccount.BlobEndpoint}/{containerName}");
            var container = new CloudBlobContainer(containerUri, DevelopmentStorageAccount.Credentials);
            if (!await container.ExistsAsync())
            {
                await container.CreateIfNotExistsAsync();
            }
        }
        public async Task DeleteContainer(string containerName)
        {
            var containerUri = new Uri($"{DevelopmentStorageAccount.BlobEndpoint}/{containerName}");
            var container = new CloudBlobContainer(containerUri, DevelopmentStorageAccount.Credentials);
            if (await container.ExistsAsync())
            {
                await container.DeleteIfExistsAsync();
            }
        }
    }
}