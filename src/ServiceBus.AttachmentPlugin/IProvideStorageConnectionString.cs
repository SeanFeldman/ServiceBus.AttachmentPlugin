namespace ServiceBus.AttachmentPlugin
{
    /// <summary>
    /// Storage account connection string provider.
    /// </summary>
    public interface IProvideStorageConnectionString
    {
        /// <summary>
        /// Connection string for storage account to be used.
        /// </summary>
        string GetConnectionString();
    }
}