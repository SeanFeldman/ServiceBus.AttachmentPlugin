namespace ServiceBus.AttachmentPlugin.Tests
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Azure.Storage;
    using Azure.Storage.Blobs;
    using Azure.Storage.Blobs.Models;
    using Azure.Storage.Sas;

    public class AzureStorageEmulatorFixture
    {
        // development account
        public const string TestingStorageAccountConnectionString = "UseDevelopmentStorage=true;";

        public static string GetBlobEndpoint()
        {
            return "http://127.0.0.1:10000/devstoreaccount1";
        }

        public static async Task<string> GetContainerSas(string containerName)
        {
            // get container
            var containerClient = new BlobContainerClient(TestingStorageAccountConnectionString, containerName);
            if (!await containerClient.ExistsAsync())
            {
                await containerClient.CreateIfNotExistsAsync();
            }

            await containerClient.GetPropertiesAsync();
            BlobContainerAccessPolicy permissionsFound = await containerClient.GetAccessPolicyAsync();

            // create access policy and store it
            var accessPolicyId = "test-policy";

            var blobSignedIdentifier = permissionsFound.SignedIdentifiers.FirstOrDefault(x=> x.Id == accessPolicyId);
            if (blobSignedIdentifier is null)
            {
                var permissions = new BlobSignedIdentifier
                {
                    Id =  accessPolicyId,
                    AccessPolicy = new BlobAccessPolicy
                    {
                        Permissions =  "acrw"
                    }
                };

                await containerClient.SetAccessPolicyAsync(PublicAccessType.None, new []{permissions});
            }
            else
            {
                blobSignedIdentifier.AccessPolicy.ExpiresOn = DateTimeOffset.UtcNow.AddDays(1);
                await containerClient.SetAccessPolicyAsync(PublicAccessType.None, new []{blobSignedIdentifier});
            }
            
            // create SAS with policy
            //return containerClient.sas.GetSharedAccessSignature(null, accessPolicyId);
            
            var blobSasBuilder = new BlobSasBuilder
            {
                Identifier = accessPolicyId,
                BlobContainerName = containerName,
                // StartsOn = DateTimeOffset.UtcNow.Subtract(TimeSpan.FromMinutes(1)),
                // ExpiresOn = DateTimeOffset.UtcNow.AddDays(1)
            };
            //blobSasBuilder.SetPermissions(BlobSasPermissions.Add | BlobSasPermissions.Create | BlobSasPermissions.Read | BlobSasPermissions.Write);
            var blobSasQueryParameters = blobSasBuilder.ToSasQueryParameters(new StorageSharedKeyCredential("devstoreaccount1", "Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw=="));
            var fullUri = $"{GetBlobEndpoint()}?{blobSasQueryParameters}";
            return fullUri;
        }

        public async Task CreateContainer(string containerName)
        {
            var container = new BlobContainerClient(TestingStorageAccountConnectionString, containerName);
            if (!await container.ExistsAsync())
            {
                await container.CreateIfNotExistsAsync();
            }
        }
        public async Task DeleteContainer(string containerName)
        {
            var container = new BlobContainerClient(TestingStorageAccountConnectionString, containerName);
            if (await container.ExistsAsync())
            {
                await container.DeleteIfExistsAsync();
            }
        }
    }
}