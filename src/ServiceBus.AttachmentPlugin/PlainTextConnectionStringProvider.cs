namespace ServiceBus.AttachmentPlugin
{
    /// <summary>
    /// Static connection string provider.
    /// </summary>
    public class PlainTextConnectionStringProvider : IProvideStorageConnectionString
    {
        readonly string connectionString;

        /// <summary></summary>
        /// <param name="connectionString">Storage connection string to use in plain text.</param>
        public PlainTextConnectionStringProvider(string connectionString)
        {
            this.connectionString = connectionString;
        }

        /// <summary>
        /// Return connection string for Storage account.
        /// </summary>
        public string GetConnectionString()
        {
            return connectionString;
        }
    }
}