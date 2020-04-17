namespace Microsoft.Azure.ServiceBus
{
    /// <summary>
    /// 
    /// </summary>
    public static class AzureStorageAttachmentConfigurationConstants
    {
        /// <summary>
        /// Default storage container name
        /// </summary>
        public const string DefaultContainerName = "attachments";

        /// <summary>
        /// Default message user property to use for blob URI
        /// </summary>
        public const string DefaultMessagePropertyToIdentifyAttachmentBlob = "$attachment.blob";

        /// <summary>
        /// Default message property which contains the SAS URI used to fetch message body from blob.
        /// </summary>
        public const string DefaultMessagePropertyToIdentitySasUri = "$attachment.sas.uri";
    }
}
