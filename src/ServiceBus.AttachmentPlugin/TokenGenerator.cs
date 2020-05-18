namespace ServiceBus.AttachmentPlugin
{
    using System;
    using Azure.Storage.Blobs.Specialized;
    using Microsoft.Azure.Storage.Blob;

    static class TokenGenerator
    {
        internal static string GetBlobSasUri(BlockBlobClient blob, TimeSpan timeSpan)
        {
            // TODO: review https://docs.microsoft.com/en-us/azure/storage/blobs/storage-blob-user-delegation-sas-create-dotnet
            
            //Set the expiry time and permissions for the blob.
            //In this case the start time is specified as a few minutes in the past, to mitigate clock skew.
            //The shared access signature will be valid immediately.
            var now = DateTime.UtcNow;
            var sasConstraints = new SharedAccessBlobPolicy
            {
                SharedAccessStartTime = now.AddMinutes(-5),
                SharedAccessExpiryTime = now.Add(timeSpan),
                Permissions = SharedAccessBlobPermissions.Delete | SharedAccessBlobPermissions.Read
            };
            //Generate the shared access signature on the blob, setting the constraints directly on the signature.
            var sasBlobToken = blob.GetSharedAccessSignature(sasConstraints);

            //Return the URI string for the container, including the SAS token.
            return blob.Uri + sasBlobToken;
        }
    }
}