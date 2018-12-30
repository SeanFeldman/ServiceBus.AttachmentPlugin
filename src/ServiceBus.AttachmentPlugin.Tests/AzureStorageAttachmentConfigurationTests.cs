namespace ServiceBus.AttachmentPlugin.Tests
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Azure.ServiceBus;
    using Xunit;

    public class AzureStorageAttachmentConfigurationTests
    {
        [Fact]
        public async Task Should_apply_defaults_for_missing_arguments()
        {
            var configuration = new AzureStorageAttachmentConfiguration(new PlainTextConnectionStringProvider("connectionString"))
                .WithSasUri();
            Assert.Equal("connectionString", await configuration.ConnectionStringProvider.GetConnectionString());
            Assert.NotEmpty(configuration.ContainerName);
            Assert.NotEmpty(configuration.MessagePropertyToIdentifyAttachmentBlob);
            Assert.Equal(AzureStorageAttachmentConfigurationExtensions.DefaultSasTokenValidationTime.Days, configuration.SasTokenValidationTime.Value.Days);
            Assert.Equal(AzureStorageAttachmentConfigurationExtensions.DefaultMessagePropertyToIdentitySasUri, configuration.MessagePropertyForSasUri);
            Assert.True(configuration.MessageMaxSizeReachedCriteria(new Message()));
        }

        [Fact]
        public void Should_not_accept_negative_token_validation_time() =>
            Assert.Throws<ArgumentException>(() => new AzureStorageAttachmentConfiguration(new PlainTextConnectionStringProvider("connectionString")).WithSasUri(sasTokenValidationTime: TimeSpan.FromHours(-4)));

        [Fact]
        public void Should_throw_when_embedded_SAS_option_is_used_with_container_SAS() =>
            Assert.Throws<Exception>(() => new AzureStorageAttachmentConfiguration(new SharedAccessSignature("https://container", "?qs")).WithSasUri());
    }
}