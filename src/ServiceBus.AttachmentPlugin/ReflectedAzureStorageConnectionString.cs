namespace Microsoft.Azure.ServiceBus
{
    using System;
    using global::Azure.Storage;

    internal class ReflectedAzureStorageConnectionString
    {
        static Type storageConnectionStringType = Type.GetType("Azure.Storage.StorageConnectionString, Azure.Storage.Common") ?? throw new Exception("`Azure.Storage.StorageConnectionString, Azure.Storage.Common` is not found");
        object storageConnectionStringObject;

        public static ReflectedAzureStorageConnectionString Create(string connectionString)
        {
            var storageConnectionStringObject = storageConnectionStringType!
                    .GetMethod("Parse", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)!
                .Invoke(null, new object[] { connectionString });

            if (storageConnectionStringObject is null)
            {
                throw new Exception($"Failed to initialize {nameof(ReflectedAzureStorageConnectionString)}.");
            }

            return new ReflectedAzureStorageConnectionString(storageConnectionStringObject!);
        }

        public StorageSharedKeyCredential Credentials => Get<StorageSharedKeyCredential>("Credentials");
        public string EndpointSuffix => Get<string>("EndpointSuffix");
        public Uri BlobEndpoint => Get<Uri>("BlobEndpoint");
        public bool IsDevStoreAccount => Get<bool>("IsDevStoreAccount");
        public (Uri primary, Uri secondary) BlobStorageUri => Get<(Uri, Uri)>("BlobStorageUri");

        ReflectedAzureStorageConnectionString(object storageConnectionStringObject)
        {
            this.storageConnectionStringObject = storageConnectionStringObject;
        }

        T Get<T>(string propertyName, bool isPublic = false)
        {
            var propertyInfo = storageConnectionStringType.GetProperty(propertyName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
            if (propertyInfo is null)
            {
                throw new Exception($"{storageConnectionStringType.FullName} does not contain a property '{propertyName}'.");
            }
		
            return (T)propertyInfo.GetValue(storageConnectionStringObject)!;
        }
    }
}