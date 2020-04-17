namespace Microsoft.Azure.ServiceBus
{
    using System;

    /// <summary>
    /// Extension method for setting up the SAS uri configuration.
    /// </summary>
    public static class AzureStorageAttachmentConfigurationExtensions
    {
        internal static readonly TimeSpan DefaultSasTokenValidationTime = TimeSpan.FromDays(7);

        /// <summary>
        /// Adds blob SAS URI configuration.
        /// </summary>
        /// <param name="azureStorageAttachmentConfiguration"></param>
        /// <param name="messagePropertyToIdentifySasUri">The <see cref="Message"/> user property used for blob SAS URI.</param>
        /// <param name="sasTokenValidationTime">The time blob SAS URI is valid for. Default value is 7 days.</param>
        /// <returns></returns>
        public static AzureStorageAttachmentConfiguration WithBlobSasUri(
            this AzureStorageAttachmentConfiguration azureStorageAttachmentConfiguration,
            string messagePropertyToIdentifySasUri = AzureStorageAttachmentConfigurationConstants.DefaultMessagePropertyToIdentitySasUri,
            TimeSpan? sasTokenValidationTime = null)
        {
            if (azureStorageAttachmentConfiguration.UsingSas)
            {
                throw new Exception("Invalid configuration: .WithBlobSasUri() requires account shared key and cannot be used with service/container Shared Access Signature.");
            }

            if (sasTokenValidationTime == null)
            {
                sasTokenValidationTime = DefaultSasTokenValidationTime;
            }
            Guard.AgainstNegativeOrZeroTimeSpan(nameof(sasTokenValidationTime), sasTokenValidationTime);

            azureStorageAttachmentConfiguration.MessagePropertyForBlobSasUri = messagePropertyToIdentifySasUri;
            azureStorageAttachmentConfiguration.BlobSasTokenValidationTime = sasTokenValidationTime.Value;

            return azureStorageAttachmentConfiguration;
        }

        /// <summary>
        /// Allow attachment blob name overriding.
        /// </summary>
        /// <param name="azureStorageAttachmentConfiguration"></param>
        /// <param name="blobNameResolver">A custom blob name resolver to override the default name set to a GUID.</param>
        /// <returns></returns>
        public static AzureStorageAttachmentConfiguration OverrideBlobName(
            this AzureStorageAttachmentConfiguration azureStorageAttachmentConfiguration,
            Func<Message, string> blobNameResolver)
        {
            Guard.AgainstNull(nameof(blobNameResolver), blobNameResolver);

            azureStorageAttachmentConfiguration.BlobNameResolver = BlobNameResolver;

            return azureStorageAttachmentConfiguration;

            string BlobNameResolver(Message message)
            {
                try
                {
                    return blobNameResolver(message);
                }
                catch (Exception exception)
                {
                    throw new Exception($"An exception occurred when executing the {nameof(blobNameResolver)} delegate.", exception);
                }
            }
        }

        /// <summary>
        /// Allow body replacement override.
        /// <remarks>
        /// By default, message body is replaced with null.
        /// </remarks>
        /// </summary>
        /// <param name="azureStorageAttachmentConfiguration"></param>
        /// <param name="bodyReplacer">A custom body replacer.</param>
        /// <returns></returns>
        public static AzureStorageAttachmentConfiguration OverrideBody(
            this AzureStorageAttachmentConfiguration azureStorageAttachmentConfiguration,
            Func<Message, byte[]> bodyReplacer)
        {
            Guard.AgainstNull(nameof(bodyReplacer), bodyReplacer);

            azureStorageAttachmentConfiguration.BodyReplacer = BodyReplacer;

            return azureStorageAttachmentConfiguration;

            byte[] BodyReplacer(Message message)
            {
                try
                {
                    return bodyReplacer(message);
                }
                catch (Exception exception)
                {
                    throw new Exception($"An exception occurred when executing {nameof(bodyReplacer)} delegate.", exception);
                }
            }
        }
    }
}