namespace ServiceBus.AttachmentPlugin.Tests
{
    using PublicApiGenerator;
    using Xunit;

    public class ApiApprovals
    {
        [Fact]
        public void AzureStorageAttachmentPlugin()
        {
            var publicApi = typeof(AzureStorageAttachment).Assembly.GeneratePublicApi(new ApiGeneratorOptions
            {
                WhitelistedNamespacePrefixes = new[] {"Microsoft.Azure.ServiceBus."},
                ExcludeAttributes = new[]
                {
                    "System.Runtime.Versioning.TargetFrameworkAttribute",
                    "System.Reflection.AssemblyMetadataAttribute"
                }
            });

            Approver.Verify(publicApi);
        }
    }
}