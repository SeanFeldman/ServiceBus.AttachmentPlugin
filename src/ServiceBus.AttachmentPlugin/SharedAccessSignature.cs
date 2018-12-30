namespace Microsoft.Azure.ServiceBus
{
    /// <summary>
    /// Shared Access Signature
    /// </summary>
    public class SharedAccessSignature
    {
        /// <summary>
        /// Query String representation of SAS
        /// </summary>
        public string QueryString { get; }

        /// <summary>
        /// Construct a new SAS using SAS query string only.
        /// <remarks>Query string can include Stored Access Policy.</remarks>
        /// </summary>
        /// <param name="queryString"></param>
        public SharedAccessSignature(string queryString)
        {
            Guard.AgainstEmpty(nameof(queryString), queryString);

            QueryString = queryString;
        }
    }
}