namespace ServiceBus.AttachmentPlugin.Tests
{
    using Microsoft.Azure.ServiceBus;
    using Xunit;

    public class AzureStorageAttachmentConfigurationTests
    {
        [Fact]
        public void Should_apply_defaults_for_missing_arguments()
        {
            var configuration = new AzureStorageAttachmentConfiguration("connectionString");
            Assert.Equal("connectionString", configuration.ConnectionString);
            Assert.NotEmpty(configuration.ContainerName);
            Assert.NotEmpty(configuration.MessagePropertyToIdentifyAttachmentBlob);
            Assert.True(configuration.MessageMaxSizeReachedCriteria(new Message()));
        }
    }
}