namespace Microsoft.Azure.ServiceBus
{
    /// <summary>
    /// Shared Access Signature
    /// </summary>
    public class SharedAccessSignature
    {
        /// <summary>
        /// Container URI w/o query string.
        /// </summary>
        public string ContainerUri { get; }

        /// <summary>
        /// Query String representation of SAS
        /// </summary>
        public string QueryString { get; }

        /// <summary>
        /// Construct a new SAS using SAS query string only.
        /// <remarks>Query string can include Stored Access Policy.</remarks>
        /// </summary>
        /// <param name="containerUri"></param>
        /// <param name="queryString"></param>
        public SharedAccessSignature(string containerUri, string queryString)
        {
            Guard.AgainstEmpty(nameof(containerUri), containerUri);
            Guard.AgainstEmpty(nameof(queryString), queryString);

            ContainerUri = containerUri;
            QueryString = queryString;
        }
    }
}