using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using CanLove_Backend.Data.Contexts;
using CanLove_Backend.Data.Models.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace CanLove_Backend.Services.Shared;

public class BlobService : IBlobService
{
    private readonly CanLoveDbContext _context;
    private readonly BlobServiceClient _blobServiceClient;
    private readonly string _storageAccountName;
    private readonly IConfiguration _configuration;

    public BlobService(CanLoveDbContext context, IConfiguration configuration, IHostEnvironment environment)
    {
        _context = context;
        _configuration = configuration;

        // 取得儲存帳戶名稱（從連接字串或配置）
        var connectionString = _configuration.GetConnectionString("AzureBlobStorage");
        
        // 判斷使用方式：本地開發用連接字串，生產環境用 Managed Identity
        if (environment.IsDevelopment() && !string.IsNullOrEmpty(connectionString) && connectionString.Contains("AccountKey"))
        {
            // 本地開發：使用連接字串
            _blobServiceClient = new BlobServiceClient(connectionString);
            
            // 從連接字串解析儲存帳戶名稱
            var accountNameMatch = System.Text.RegularExpressions.Regex.Match(
                connectionString, 
                @"AccountName=([^;]+)");
            _storageAccountName = accountNameMatch.Success 
                ? accountNameMatch.Groups[1].Value 
                : "unknown";
        }
        else
        {
            // 生產環境：使用 Managed Identity
            // 從連接字串或配置取得儲存帳戶名稱
            if (!string.IsNullOrEmpty(connectionString))
            {
                var accountNameMatch = System.Text.RegularExpressions.Regex.Match(
                    connectionString, 
                    @"AccountName=([^;]+)");
                _storageAccountName = accountNameMatch.Success 
                    ? accountNameMatch.Groups[1].Value 
                    : _configuration["AzureStorage:AccountName"] ?? throw new InvalidOperationException("無法取得儲存帳戶名稱");
            }
            else
            {
                _storageAccountName = _configuration["AzureStorage:AccountName"] 
                    ?? throw new InvalidOperationException("AzureStorage:AccountName 未設定");
            }

            // 使用 Managed Identity 認證
            var credential = new DefaultAzureCredential();
            _blobServiceClient = new BlobServiceClient(
                new Uri($"https://{_storageAccountName}.blob.core.windows.net"),
                credential);
        }
    }

    public async Task<BlobStorage> UploadFileAsync(
        Stream fileStream,
        string containerName,
        string fileName,
        string contentType,
        int? uploadedBy = null,
        bool isTemp = false)
    {
        // 確保容器存在
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        await containerClient.CreateIfNotExistsAsync(PublicAccessType.None);

        // 取得檔案大小
        var fileSize = fileStream.Length;
        
        // 產生唯一的blob名稱
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var random = Guid.NewGuid().ToString("N").Substring(0, 8);
        var fileExtension = Path.GetExtension(fileName);
        var blobName = $"{Path.GetFileNameWithoutExtension(fileName)}_{timestamp}_{random}{fileExtension}";

        // 上傳檔案到Azure Blob Storage
        var blobClient = containerClient.GetBlobClient(blobName);
        fileStream.Position = 0;
        
        await blobClient.UploadAsync(fileStream, new BlobUploadOptions
        {
            HttpHeaders = new BlobHttpHeaders
            {
                ContentType = contentType
            }
        });

        // 建立BlobStorage記錄
        var blobStorage = new BlobStorage
        {
            ContainerName = containerName,
            BlobName = blobName,
            OriginalFileName = fileName,
            FileExtension = fileExtension.TrimStart('.'),
            ContentType = contentType,
            FileSize = fileSize,
            StorageAccount = _storageAccountName,
            BlobUri = blobClient.Uri.ToString(),
            BlobUrl = blobClient.Uri.ToString(), // 如果需要SAS URL，可以另外產生
            UploadedBy = uploadedBy,
            UploadedAt = DateTime.UtcNow,
            IsTemp = isTemp,
            ExpiresAt = isTemp ? DateTime.UtcNow.AddDays(7) : null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.BlobStorage.Add(blobStorage);
        await _context.SaveChangesAsync();

        return blobStorage;
    }

    public async Task<Stream> DownloadFileAsync(int blobId)
    {
        var blobStorage = await _context.BlobStorage
            .FirstOrDefaultAsync(b => b.BlobId == blobId && !b.Deleted);

        if (blobStorage == null)
        {
            throw new FileNotFoundException($"找不到Blob ID: {blobId}");
        }

        var containerClient = _blobServiceClient.GetBlobContainerClient(blobStorage.ContainerName);
        var blobClient = containerClient.GetBlobClient(blobStorage.BlobName);

        var stream = new MemoryStream();
        await blobClient.DownloadToAsync(stream);
        stream.Position = 0;

        return stream;
    }

    public async Task<string> GetFileUrlAsync(int blobId)
    {
        var blobStorage = await _context.BlobStorage
            .FirstOrDefaultAsync(b => b.BlobId == blobId && !b.Deleted);

        if (blobStorage == null)
        {
            throw new FileNotFoundException($"找不到Blob ID: {blobId}");
        }

        // 如果已經有URL，直接返回
        if (!string.IsNullOrEmpty(blobStorage.BlobUrl))
        {
            return blobStorage.BlobUrl;
        }

        // 產生SAS URL（如果需要）
        var containerClient = _blobServiceClient.GetBlobContainerClient(blobStorage.ContainerName);
        var blobClient = containerClient.GetBlobClient(blobStorage.BlobName);

        // 產生有效期24小時的SAS Token
        if (blobClient.CanGenerateSasUri)
        {
            var sasBuilder = new BlobSasBuilder
            {
                BlobContainerName = blobStorage.ContainerName,
                BlobName = blobStorage.BlobName,
                Resource = "b", // blob
                ExpiresOn = DateTimeOffset.UtcNow.AddHours(24)
            };

            sasBuilder.SetPermissions(BlobSasPermissions.Read);

            var sasUri = blobClient.GenerateSasUri(sasBuilder);
            return sasUri.ToString();
        }

        return blobClient.Uri.ToString();
    }

    public async Task<bool> DeleteFileAsync(int blobId)
    {
        var blobStorage = await _context.BlobStorage
            .FirstOrDefaultAsync(b => b.BlobId == blobId && !b.Deleted);

        if (blobStorage == null)
        {
            return false;
        }

        try
        {
            // 從Azure Blob Storage刪除
            var containerClient = _blobServiceClient.GetBlobContainerClient(blobStorage.ContainerName);
            var blobClient = containerClient.GetBlobClient(blobStorage.BlobName);
            await blobClient.DeleteIfExistsAsync();

            // 軟刪除資料庫記錄
            blobStorage.Deleted = true;
            blobStorage.DeletedAt = DateTime.UtcNow;
            blobStorage.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<BlobStorage> DownloadFromUrlAsync(
        string url,
        string containerName,
        string fileName,
        int? uploadedBy = null)
    {
        using var httpClient = new System.Net.Http.HttpClient();
        var response = await httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        using var stream = await response.Content.ReadAsStreamAsync();
        var contentType = response.Content.Headers.ContentType?.MediaType ?? "application/octet-stream";

        return await UploadFileAsync(stream, containerName, fileName, contentType, uploadedBy, false);
    }
}

