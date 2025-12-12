using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using CanLove_Backend.Domain.Case.Exceptions;

namespace CanLove_Backend.Core.Middleware;

/// <summary>
/// 全局錯誤處理中介軟體
/// 捕獲所有未處理的異常，返回友好的錯誤回應
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IWebHostEnvironment _environment;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IWebHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        // 業務邏輯異常使用較低的日誌等級（Warning），其他異常使用 Error
        if (IsBusinessException(exception))
        {
            _logger.LogWarning(
                "業務邏輯異常：{ExceptionType} - {Message} | Path: {Path} | User: {User}",
                exception.GetType().Name,
                exception.Message,
                context.Request.Path,
                context.User?.Identity?.Name ?? "Anonymous");
        }
        else
        {
            // 記錄錯誤日誌（包含完整堆疊追蹤）
            _logger.LogError(exception, 
                "未處理的異常：{Message} | Path: {Path} | Method: {Method} | User: {User}",
                exception.Message,
                context.Request.Path,
                context.Request.Method,
                context.User?.Identity?.Name ?? "Anonymous");
        }

        // 決定 HTTP 狀態碼
        var statusCode = GetStatusCode(exception);
        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "text/html; charset=utf-8";

        // 檢查是否為 API 請求（根據 Accept 標頭）
        var isApiRequest = context.Request.Headers["Accept"]
            .ToString()
            .Contains("application/json", StringComparison.OrdinalIgnoreCase) ||
            context.Request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase);

        if (isApiRequest)
        {
            // API 請求：返回 JSON
            await WriteJsonResponseAsync(context, exception, statusCode);
        }
        else
        {
            // HTML 請求：重定向到錯誤頁面
            // 業務邏輯異常的訊息可以傳遞給錯誤頁面
            var errorMessage = IsBusinessException(exception) 
                ? Uri.EscapeDataString(exception.Message) 
                : string.Empty;
            context.Response.Redirect($"/Home/Error?statusCode={(int)statusCode}&message={errorMessage}");
        }
    }

    private static HttpStatusCode GetStatusCode(Exception exception)
    {
        return exception switch
        {
            // 自訂個案異常處理
            CaseBasicNotFoundException => HttpStatusCode.NotFound,
            CaseBasicLockedException => HttpStatusCode.Conflict,           // 409 衝突（資源被鎖定）
            CaseBasicInvalidStatusException => HttpStatusCode.BadRequest,  // 400 狀態不允許
            CaseBasicValidationException => HttpStatusCode.BadRequest,     // 400 驗證失敗
            CaseBasicSaveException => HttpStatusCode.InternalServerError,  // 500 儲存失敗
            CaseBasicException => HttpStatusCode.BadRequest,               // 400 其他個案異常
            
            // 標準異常處理
            ArgumentException or ArgumentNullException => HttpStatusCode.BadRequest,
            UnauthorizedAccessException => HttpStatusCode.Unauthorized,
            KeyNotFoundException or FileNotFoundException => HttpStatusCode.NotFound,
            NotSupportedException => HttpStatusCode.MethodNotAllowed,
            _ => HttpStatusCode.InternalServerError
        };
    }

    /// <summary>
    /// 判斷是否為業務邏輯異常（這些異常的訊息可以直接顯示給用戶）
    /// </summary>
    private static bool IsBusinessException(Exception exception)
    {
        return exception is CaseBasicException;
    }

    private async Task WriteJsonResponseAsync(HttpContext context, Exception exception, HttpStatusCode statusCode)
    {
        context.Response.ContentType = "application/json; charset=utf-8";

        // 業務邏輯異常的訊息可以直接顯示給用戶
        // 其他異常只在開發環境顯示詳細訊息
        var showMessage = IsBusinessException(exception) || _environment.IsDevelopment();
        
        var errorResponse = new
        {
            error = new
            {
                message = showMessage 
                    ? exception.Message 
                    : "發生錯誤，請稍後再試",
                statusCode = (int)statusCode,
                // 業務邏輯異常不需要顯示堆疊追蹤，只有開發環境才顯示
                details = _environment.IsDevelopment() && !IsBusinessException(exception)
                    ? exception.ToString() 
                    : null
            }
        };

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        var json = JsonSerializer.Serialize(errorResponse, jsonOptions);
        await context.Response.WriteAsync(json);
    }
}

