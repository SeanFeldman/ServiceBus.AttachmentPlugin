namespace ServiceBus.AttachmentPlugin.Tests
{
    using PublicApiGenerator;
    using Xunit;

    public class ApiApprovals
    {
        [Fact]
        public void AzureStorageAttachmentPlugin()
        {
            var publicApi = ApiGenerator.GeneratePublicApi(typeof(AzureStorageAttachment).Assembly,
                new ApiGeneratorOptions
                {
                    WhitelistedNamespacePrefixes = new[] {"Microsoft.Azure.ServiceBus."},
                    ExcludeAttributes = new[] {"System.Runtime.Versioning.TargetFrameworkAttribute"}
                });

            Approver.Verify(publicApi);
        }
    }
}