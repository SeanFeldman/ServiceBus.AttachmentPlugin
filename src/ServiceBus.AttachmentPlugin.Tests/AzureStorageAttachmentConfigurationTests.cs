namespace ServiceBus.AttachmentPlugin.Tests
{
    using System;
    using Microsoft.Azure.ServiceBus;
    using Xunit;

    public class AzureStorageAttachmentConfigurationTests
    {
        const string ConnectionString = "AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;DefaultEndpointsProtocol=http;BlobEndpoint=http://127.0.0.1:10000/devstoreaccount1";

        [Fact]
        public void Should_apply_defaults_for_missing_arguments()
        {
            var configuration = new AzureStorageAttachmentConfiguration(ConnectionString)
                .WithBlobSasUri();
            Assert.NotEmpty(configuration.ContainerName);
            Assert.NotEmpty(configuration.MessagePropertyToIdentifyAttachmentBlob);
            Assert.Equal(AzureStorageAttachmentConfigurationExtensions.DefaultSasTokenValidationTime.Days, configuration.BlobSasTokenValidationTime!.Value.Days);
            Assert.Equal(AzureStorageAttachmentConfigurationConstants.DefaultMessagePropertyToIdentitySasUri, configuration.MessagePropertyForBlobSasUri);
            Assert.True(configuration.MessageMaxSizeReachedCriteria(new Message()));
        }

        [Fact]
        public void Should_not_accept_negative_token_validation_time() =>
            Assert.Throws<ArgumentException>(() => new AzureStorageAttachmentConfiguration(ConnectionString)
                .WithBlobSasUri(sasTokenValidationTime: TimeSpan.FromHours(-4)));

        [Fact]
        public void Should_throw_when_embedded_SAS_option_is_used_with_container_SAS() =>
            Assert.Throws<Exception>(() => new AzureStorageAttachmentConfiguration(
                "http://127.0.0.1:10000/devstoreaccount1?sv=2018-03-28&sr=c&sig=5XxlRKoP4yEmibM2HhJlQuV7MG3rYgQXD89mLpNp%2F24%3D", "devstoreaccount1").WithBlobSasUri());
    }
}