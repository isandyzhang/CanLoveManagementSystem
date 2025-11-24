using System.IO;
using System.Threading.Tasks;
using CanLove_Backend.Infrastructure.Storage.Blob;
using CanLove_Backend.Domain.Case.Models.Basic;

namespace CanLove_Backend.Infrastructure.Storage.Blob;

public interface IBlobService
{
    /// <summary>
    /// 上傳檔案到Blob Storage
    /// </summary>
    Task<BlobStorage> UploadFileAsync(
        Stream fileStream,
        string containerName,
        string fileName,
        string contentType,
        int? uploadedBy = null,
        bool isTemp = false);

    /// <summary>
    /// 下載檔案
    /// </summary>
    Task<Stream> DownloadFileAsync(int blobId);

    /// <summary>
    /// 取得檔案URL
    /// </summary>
    Task<string> GetFileUrlAsync(int blobId);

    /// <summary>
    /// 刪除檔案
    /// </summary>
    Task<bool> DeleteFileAsync(int blobId);

    /// <summary>
    /// 從URL下載並儲存
    /// </summary>
    Task<BlobStorage> DownloadFromUrlAsync(
        string url,
        string containerName,
        string fileName,
        int? uploadedBy = null);
}

