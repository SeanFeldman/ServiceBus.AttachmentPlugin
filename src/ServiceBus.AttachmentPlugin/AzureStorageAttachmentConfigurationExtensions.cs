namespace ServiceBus.AttachmentPlugin
{
    using System;

    /// <summary>
    /// Extension method for setting up the SAS uri configuration.
    /// </summary>
    public static class AzureStorageAttachmentConfigurationExtensions
    {
        const string DefaultMessagePropertyToIdentitySasUri = "$attachment.sas.uri";
        static TimeSpan DefaultSasTokenValidationTime = TimeSpan.FromDays(7);

        /// <summary>
        /// Adds sas uri configuration.
        /// </summary>
        /// <param name="azureStorageAttachmentConfiguration"></param>
        /// <param name="messagePropertyToIdentifySasUri">The message property where the sas uri is set.</param>
        /// <param name="sasTokenValidationTime">The time the sas uri is valid.</param>
        /// <returns></returns>
        public static AzureStorageAttachmentConfiguration WithSasUri(
            this AzureStorageAttachmentConfiguration azureStorageAttachmentConfiguration, 
            string messagePropertyToIdentifySasUri = DefaultMessagePropertyToIdentitySasUri, 
            TimeSpan? sasTokenValidationTime = null)
        {
            if (sasTokenValidationTime == null)
            {
                sasTokenValidationTime = DefaultSasTokenValidationTime;
            }
            Guard.AgainstNegativeTime(nameof(sasTokenValidationTime), sasTokenValidationTime);

            azureStorageAttachmentConfiguration.MessagePropertyForSasUri = messagePropertyToIdentifySasUri;
            azureStorageAttachmentConfiguration.SasTokenValidationTime = sasTokenValidationTime.Value;

            return azureStorageAttachmentConfiguration;
        }
    }
}