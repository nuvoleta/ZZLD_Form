using ZZLD_Form.Shared.DTOs;

namespace ZZLD_Form.Infrastructure.Storage;

/// <summary>
/// Service for Azure Blob Storage operations
/// </summary>
public interface IBlobStorageService
{
    /// <summary>
    /// Uploads a form PDF to blob storage with metadata
    /// </summary>
    /// <param name="pdfBytes">PDF content as bytes</param>
    /// <param name="metadata">Form metadata</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Blob name, download URL with SAS token, and expiration time</returns>
    Task<(string BlobName, string DownloadUrl, DateTime ExpiresAt)> UploadFormAsync(
        byte[] pdfBytes,
        FormMetadata metadata,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a form by its ID
    /// </summary>
    /// <param name="formId">Form identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Blob name, download URL with SAS token, expiration time, and metadata</returns>
    Task<(string BlobName, string DownloadUrl, DateTime ExpiresAt, FormMetadata Metadata)> GetFormByIdAsync(
        string formId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates a SAS token for a blob
    /// </summary>
    /// <param name="blobName">Blob name</param>
    /// <param name="expiryHours">Token expiry in hours</param>
    /// <returns>Download URL with SAS token</returns>
    Task<string> GenerateSasTokenAsync(string blobName, int expiryHours = 24);

    /// <summary>
    /// Checks if a blob exists
    /// </summary>
    /// <param name="blobName">Blob name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if blob exists</returns>
    Task<bool> BlobExistsAsync(string blobName, CancellationToken cancellationToken = default);
}
