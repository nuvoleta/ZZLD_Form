using Azure;
using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Retry;
using System.Text.Json;
using ZZLD_Form.Infrastructure.Configuration;
using ZZLD_Form.Shared.Constants;
using ZZLD_Form.Shared.DTOs;

namespace ZZLD_Form.Infrastructure.Storage;

/// <summary>
/// Implementation of blob storage service with retry logic
/// </summary>
public class BlobStorageService : IBlobStorageService
{
    private readonly BlobContainerClient _containerClient;
    private readonly AzureStorageOptions _options;
    private readonly AsyncRetryPolicy _retryPolicy;

    public BlobStorageService(IOptions<AzureStorageOptions> options)
    {
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));

        // Initialize blob container client
        if (_options.UseManagedIdentity)
        {
            var blobServiceClient = new BlobServiceClient(
                new Uri($"https://{_options.AccountName}.blob.core.windows.net"),
                new DefaultAzureCredential());
            _containerClient = blobServiceClient.GetBlobContainerClient(_options.ContainerName);
        }
        else
        {
            _containerClient = new BlobContainerClient(_options.ConnectionString, _options.ContainerName);
        }

        // Configure retry policy with Polly
        _retryPolicy = Policy
            .Handle<RequestFailedException>()
            .Or<Exception>()
            .WaitAndRetryAsync(
                FormConstants.DefaultRetryCount,
                retryAttempt => TimeSpan.FromMilliseconds(FormConstants.DefaultRetryDelayMs * Math.Pow(2, retryAttempt - 1)));
    }

    /// <inheritdoc/>
    public async Task<(string BlobName, string DownloadUrl, DateTime ExpiresAt)> UploadFormAsync(
        byte[] pdfBytes,
        FormMetadata metadata,
        CancellationToken cancellationToken = default)
    {
        if (pdfBytes == null || pdfBytes.Length == 0)
            throw new ArgumentException("PDF bytes cannot be null or empty", nameof(pdfBytes));

        if (metadata == null)
            throw new ArgumentNullException(nameof(metadata));

        return await _retryPolicy.ExecuteAsync(async () =>
        {
            // Ensure container exists
            await _containerClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken);

            // Generate blob name
            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            var blobName = $"{FormConstants.GeneratedFilesPath}/{timestamp}_{Guid.NewGuid():N}{FormConstants.PdfExtension}";

            // Get blob client
            var blobClient = _containerClient.GetBlobClient(blobName);

            // Set metadata and content type
            var blobMetadata = new Dictionary<string, string>
            {
                ["FormId"] = metadata.FormId,
                ["FullName"] = metadata.FullName,
                ["GeneratedAt"] = metadata.GeneratedAt.ToString("O"),
                ["EGN"] = metadata.EGN,
                ["Email"] = metadata.Email
            };

            var uploadOptions = new BlobUploadOptions
            {
                Metadata = blobMetadata,
                HttpHeaders = new BlobHttpHeaders
                {
                    ContentType = metadata.ContentType
                }
            };

            // Upload PDF
            using var stream = new MemoryStream(pdfBytes);
            await blobClient.UploadAsync(stream, uploadOptions, cancellationToken);

            // Generate SAS token
            var downloadUrl = await GenerateSasTokenAsync(blobName, _options.SasTokenValidityHours);
            var expiresAt = DateTime.UtcNow.AddHours(_options.SasTokenValidityHours);

            return (blobName, downloadUrl, expiresAt);
        });
    }

    /// <inheritdoc/>
    public async Task<(string BlobName, string DownloadUrl, DateTime ExpiresAt, FormMetadata Metadata)> GetFormByIdAsync(
        string formId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(formId))
            throw new ArgumentException("Form ID cannot be null or empty", nameof(formId));

        return await _retryPolicy.ExecuteAsync(async () =>
        {
            // List all blobs and find the one with matching formId in metadata
            var blobs = _containerClient.GetBlobsAsync(
                BlobTraits.Metadata,
                BlobStates.None,
                prefix: FormConstants.GeneratedFilesPath,
                cancellationToken: cancellationToken);

            await foreach (var blobItem in blobs)
            {
                if (blobItem.Metadata.TryGetValue("FormId", out var storedFormId) && storedFormId == formId)
                {
                    var metadata = new FormMetadata
                    {
                        FormId = formId,
                        FullName = blobItem.Metadata.GetValueOrDefault("FullName", string.Empty),
                        GeneratedAt = DateTime.Parse(blobItem.Metadata.GetValueOrDefault("GeneratedAt", DateTime.UtcNow.ToString("O"))),
                        EGN = blobItem.Metadata.GetValueOrDefault("EGN", string.Empty),
                        Email = blobItem.Metadata.GetValueOrDefault("Email", string.Empty)
                    };

                    var downloadUrl = await GenerateSasTokenAsync(blobItem.Name, _options.SasTokenValidityHours);
                    var expiresAt = DateTime.UtcNow.AddHours(_options.SasTokenValidityHours);

                    return (blobItem.Name, downloadUrl, expiresAt, metadata);
                }
            }

            throw new FileNotFoundException($"Form with ID {formId} not found");
        });
    }

    /// <inheritdoc/>
    public async Task<string> GenerateSasTokenAsync(string blobName, int expiryHours = 24)
    {
        if (string.IsNullOrWhiteSpace(blobName))
            throw new ArgumentException("Blob name cannot be null or empty", nameof(blobName));

        var blobClient = _containerClient.GetBlobClient(blobName);

        // Check if we can generate SAS token (requires account key)
        if (_options.UseManagedIdentity)
        {
            // For managed identity, return blob URL without SAS (requires appropriate RBAC permissions)
            return blobClient.Uri.ToString();
        }

        // Generate SAS token
        var sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = _options.ContainerName,
            BlobName = blobName,
            Resource = "b",
            StartsOn = DateTimeOffset.UtcNow.AddMinutes(-5),
            ExpiresOn = DateTimeOffset.UtcNow.AddHours(expiryHours)
        };

        sasBuilder.SetPermissions(BlobSasPermissions.Read);

        var sasToken = blobClient.GenerateSasUri(sasBuilder);
        return sasToken.ToString();
    }

    /// <inheritdoc/>
    public async Task<bool> BlobExistsAsync(string blobName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(blobName))
            return false;

        try
        {
            var blobClient = _containerClient.GetBlobClient(blobName);
            return await blobClient.ExistsAsync(cancellationToken);
        }
        catch
        {
            return false;
        }
    }
}
