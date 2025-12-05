using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

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
        // 記錄錯誤日誌（包含完整堆疊追蹤）
        _logger.LogError(exception, 
            "未處理的異常：{Message} | Path: {Path} | Method: {Method} | User: {User}",
            exception.Message,
            context.Request.Path,
            context.Request.Method,
            context.User?.Identity?.Name ?? "Anonymous");

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
            context.Response.Redirect($"/Home/Error?statusCode={statusCode}");
        }
    }

    private static HttpStatusCode GetStatusCode(Exception exception)
    {
        return exception switch
        {
            ArgumentException or ArgumentNullException => HttpStatusCode.BadRequest,
            UnauthorizedAccessException => HttpStatusCode.Unauthorized,
            KeyNotFoundException or FileNotFoundException => HttpStatusCode.NotFound,
            NotSupportedException => HttpStatusCode.MethodNotAllowed,
            _ => HttpStatusCode.InternalServerError
        };
    }

    private async Task WriteJsonResponseAsync(HttpContext context, Exception exception, HttpStatusCode statusCode)
    {
        context.Response.ContentType = "application/json; charset=utf-8";

        var errorResponse = new
        {
            error = new
            {
                message = _environment.IsDevelopment() 
                    ? exception.Message 
                    : "發生錯誤，請稍後再試",
                statusCode = (int)statusCode,
                details = _environment.IsDevelopment() 
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

