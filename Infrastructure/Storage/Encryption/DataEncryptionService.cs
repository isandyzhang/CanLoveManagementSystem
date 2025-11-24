using Microsoft.AspNetCore.DataProtection;

namespace CanLove_Backend.Infrastructure.Storage.Encryption;

/// <summary>
/// 資料加密服務 - 用於加密敏感資料（如身分證字號）
/// </summary>
public class DataEncryptionService
{
    private readonly IDataProtector _protector;
    private const string Purpose = "IdNumberProtection"; // 用於區分不同用途的加密

    public DataEncryptionService(IDataProtectionProvider dataProtectionProvider)
    {
        _protector = dataProtectionProvider.CreateProtector(Purpose);
    }

    /// <summary>
    /// 加密字串
    /// </summary>
    /// <param name="plainText">明文</param>
    /// <returns>加密後的 Base64 字串</returns>
    public string Encrypt(string? plainText)
    {
        if (string.IsNullOrWhiteSpace(plainText))
        {
            return string.Empty;
        }

        try
        {
            return _protector.Protect(plainText);
        }
        catch (Exception ex)
        {
            // 記錄錯誤但不拋出異常，避免影響業務流程
            // 在生產環境中應該使用 ILogger 記錄
            throw new InvalidOperationException($"加密失敗：{ex.Message}", ex);
        }
    }

    /// <summary>
    /// 解密字串
    /// </summary>
    /// <param name="encryptedText">加密後的 Base64 字串</param>
    /// <returns>解密後的明文</returns>
    public string Decrypt(string? encryptedText)
    {
        if (string.IsNullOrWhiteSpace(encryptedText))
        {
            return string.Empty;
        }

        try
        {
            return _protector.Unprotect(encryptedText);
        }
        catch (Exception ex)
        {
            // 如果解密失敗（可能是舊資料或格式錯誤），記錄並返回空字串
            // 在生產環境中應該使用 ILogger 記錄
            throw new InvalidOperationException($"解密失敗：{ex.Message}", ex);
        }
    }

    /// <summary>
    /// 安全地解密字串（失敗時返回空字串，不拋出異常）
    /// </summary>
    /// <param name="encryptedText">加密後的 Base64 字串</param>
    /// <returns>解密後的明文，失敗時返回空字串</returns>
    public string DecryptSafely(string? encryptedText)
    {
        if (string.IsNullOrWhiteSpace(encryptedText))
        {
            return string.Empty;
        }

        try
        {
            return _protector.Unprotect(encryptedText);
        }
        catch
        {
            // 解密失敗時返回空字串（可能是舊的未加密資料）
            return string.Empty;
        }
    }
}

