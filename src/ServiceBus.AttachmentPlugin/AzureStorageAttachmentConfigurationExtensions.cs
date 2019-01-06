namespace Microsoft.Azure.ServiceBus
{
    using System;

    /// <summary>
    /// Extension method for setting up the SAS uri configuration.
    /// </summary>
    public static class AzureStorageAttachmentConfigurationExtensions
    {
        internal const string DefaultMessagePropertyToIdentitySasUri = "$attachment.sas.uri";
        internal static TimeSpan DefaultSasTokenValidationTime = TimeSpan.FromDays(7);

        /// <summary>
        /// Adds SAS uri configuration.
        /// </summary>
        /// <param name="azureStorageAttachmentConfiguration"></param>
        /// <param name="messagePropertyToIdentifySasUri">The <see cref="Message"/> user property used for SAS uri.</param>
        /// <param name="sasTokenValidationTime">The time SAS uri is valid for.</param>
        /// <returns></returns>
        public static AzureStorageAttachmentConfiguration WithSasUri(
            this AzureStorageAttachmentConfiguration azureStorageAttachmentConfiguration,
            string messagePropertyToIdentifySasUri = DefaultMessagePropertyToIdentitySasUri,
            TimeSpan? sasTokenValidationTime = null)
        {
            if (azureStorageAttachmentConfiguration.UsingSas)
            {
                throw new Exception("Invalid configuration: .WithSasUri() requires account key and cannot be used with container Shared Access Signature.");
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
    }
}