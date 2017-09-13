namespace ServiceBus.AttachmentPlugin.Tests
{
    using System;
    using Microsoft.Azure.ServiceBus;
    using Xunit;

    public class AzureStorageAttachmentConfigurationTests
    {
        [Fact]
        public void Should_apply_defaults_for_missing_arguments()
        {
            var configuration = new AzureStorageAttachmentConfiguration("connectionString")
                .WithSasUri();
            Assert.Equal("connectionString", configuration.ConnectionString);
            Assert.NotEmpty(configuration.ContainerName);
            Assert.NotEmpty(configuration.MessagePropertyToIdentifyAttachmentBlob);
            Assert.Equal(TimeSpan.FromDays(7).Days, configuration.SasTokenValidationTime.Value.Days);
            Assert.Equal("$attachment.sas.uri", configuration.MessagePropertyForSasUri);
            Assert.True(configuration.MessageMaxSizeReachedCriteria(new Message()));
        }

        [Fact]
        public void Should_not_accept_negative_token_validation_time()
        {
            Assert.Throws<ArgumentException>(() => new AzureStorageAttachmentConfiguration("connectionString").WithSasUri(sasTokenValidationTime: TimeSpan.FromHours(-4)));
        }
    }
}