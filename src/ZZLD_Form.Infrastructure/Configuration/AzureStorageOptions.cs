namespace ZZLD_Form.Infrastructure.Configuration;

/// <summary>
/// Configuration options for Azure Storage
/// </summary>
public class AzureStorageOptions
{
    /// <summary>
    /// Configuration section name
    /// </summary>
    public const string SectionName = "AzureStorage";

    /// <summary>
    /// Storage account connection string
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Storage account name
    /// </summary>
    public string AccountName { get; set; } = string.Empty;

    /// <summary>
    /// Container name for ZZLD forms
    /// </summary>
    public string ContainerName { get; set; } = "zzld-form";

    /// <summary>
    /// Whether to use managed identity
    /// </summary>
    public bool UseManagedIdentity { get; set; } = false;

    /// <summary>
    /// SAS token validity in hours
    /// </summary>
    public int SasTokenValidityHours { get; set; } = 24;
}
