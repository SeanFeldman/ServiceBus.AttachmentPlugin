namespace ServiceBus.AttachmentPlugin
{
    using Microsoft.WindowsAzure.Storage.Blob;
    using System;

    static class TokenGenerator
    {
        internal static string GetBlobSasUri(CloudBlockBlob blob, TimeSpan timeSpan)
        {
            //Set the expiry time and permissions for the blob.
            //In this case the start time is specified as a few minutes in the past, to mitigate clock skew.
            //The shared access signature will be valid immediately.
            var sasConstraints = new SharedAccessBlobPolicy
            {
                SharedAccessStartTime = DateTime.UtcNow.AddMinutes(-5),
                SharedAccessExpiryTime = DateTime.UtcNow.Add(timeSpan),
                Permissions = SharedAccessBlobPermissions.Delete | SharedAccessBlobPermissions.Read
            };
            //Generate the shared access signature on the blob, setting the constraints directly on the signature.
            var sasBlobToken = blob.GetSharedAccessSignature(sasConstraints);

            //Return the URI string for the container, including the SAS token.
            return blob.Uri + sasBlobToken;
        }
    }
}
