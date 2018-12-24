namespace ServiceBus.AttachmentPlugin.Tests
{
    using PublicApiGenerator;
    using Xunit;

    public class ApiApprovals
    {
        [Fact]
        public void CompressionPlugin()
        {
            var publicApi = ApiGenerator.GeneratePublicApi(typeof(AzureStorageAttachment).Assembly,
                whitelistedNamespacePrefixes: new[] { "Microsoft.Azure.ServiceBus." },
                excludeAttributes: new[] { "System.Runtime.Versioning.TargetFrameworkAttribute" });

            Approver.Verify(publicApi);
        }
    }
}