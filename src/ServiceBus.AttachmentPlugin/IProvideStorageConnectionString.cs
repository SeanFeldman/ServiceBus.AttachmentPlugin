namespace Microsoft.Azure.ServiceBus
{
    using System.Threading.Tasks;

    /// <summary>
    /// Storage account connection string provider.
    /// </summary>
    public interface IProvideStorageConnectionString
    {
        /// <summary>
        /// Connection string for storage account to be used.
        /// </summary>
        Task<string> GetConnectionString();
    }
}