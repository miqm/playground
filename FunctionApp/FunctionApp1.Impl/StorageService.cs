using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Auth;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FunctionApp1.Impl
{
    class StorageService : IStorageService
    {
        private readonly ILogger<StorageService> _logger;
        private readonly IOptions<FunctionAppSettings> _options;

        public StorageService(ILogger<StorageService> logger, IOptions<FunctionAppSettings> options)
        {
            _logger = logger;
            _options = options;
        }
        public async Task TestStorageAsync()
        {
            _logger.LogInformation("Testing Storage {status}", "starting");
            var blobPath = _options.Value.BlobPath;
            StorageCredentials storageCredentials;

            if (blobPath.StartsWith(CloudStorageAccount.DevelopmentStorageAccount.BlobEndpoint.ToString()))
            {
                storageCredentials = CloudStorageAccount.DevelopmentStorageAccount.Credentials;
            }
            else
            {
                // Get the initial access token and the interval at which to refresh it.
                AzureServiceTokenProvider azureServiceTokenProvider = new AzureServiceTokenProvider();
                var tokenAndFrequency = TokenRenewerAsync(azureServiceTokenProvider,
                                                            CancellationToken.None).GetAwaiter().GetResult();

                // Create storage credentials using the initial token, and connect the callback function 
                // to renew the token just before it expires
                TokenCredential tokenCredential = new TokenCredential(tokenAndFrequency.Token,
                                                                        TokenRenewerAsync,
                                                                        azureServiceTokenProvider,
                                                                        tokenAndFrequency.Frequency.Value);

                storageCredentials = new StorageCredentials(tokenCredential);

            }

            var blobContainer = new CloudBlobContainer(new Uri(blobPath), storageCredentials);
            var blobObject = blobContainer.GetBlockBlobReference($"test-blob-{Guid.NewGuid().ToString()}");
            await blobObject.UploadTextAsync($"DateTime: {DateTime.UtcNow.ToString("u")}");

            _logger.LogInformation("Testing Storage {status}", "completed");
        }

        private async Task<NewTokenAndFrequency> TokenRenewerAsync(Object state, CancellationToken cancellationToken)
        {
            // Specify the resource ID for requesting Azure AD tokens for Azure Storage.
            const string StorageResource = "https://storage.azure.com/";

            // Use the same token provider to request a new token.
            var authResult = await ((AzureServiceTokenProvider)state).GetAuthenticationResultAsync(StorageResource);

            // Renew the token 5 minutes before it expires.
            var next = (authResult.ExpiresOn - DateTimeOffset.UtcNow) - TimeSpan.FromMinutes(5);
            if (next.Ticks < 0)
            {
                next = default(TimeSpan);
                _logger.LogInformation("Renewing storage token...");
            }

            // Return the new token and the next refresh time.
            return new NewTokenAndFrequency(authResult.AccessToken, next);
        }
    }

    public interface IStorageService
    {
        Task TestStorageAsync();
    }
}
