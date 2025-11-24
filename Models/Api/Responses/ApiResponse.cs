namespace CanLove_Backend.Models.Api.Responses
{
    /// <summary>
    /// 統一的 API 響應格式
    /// </summary>
    /// <typeparam name="T">資料型別</typeparam>
    public class ApiResponse<T>
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 訊息
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// 資料
        /// </summary>
        public T? Data { get; set; }

        /// <summary>
        /// 錯誤代碼
        /// </summary>
        public string? ErrorCode { get; set; }

        /// <summary>
        /// 時間戳記
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow.AddHours(8);

        /// <summary>
        /// 成功響應
        /// </summary>
        public static ApiResponse<T> SuccessResponse(T data, string message = "操作成功")
        {
            return new ApiResponse<T>
            {
                Success = true,
                Message = message,
                Data = data
            };
        }

        /// <summary>
        /// 失敗響應
        /// </summary>
        public static ApiResponse<T> ErrorResponse(string message, string? errorCode = null)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                ErrorCode = errorCode
            };
        }
    }

    /// <summary>
    /// 無資料的 API 響應
    /// </summary>
    public class ApiResponse : ApiResponse<object>
    {
        public static ApiResponse SuccessResult(string message = "操作成功")
        {
            var response = new ApiResponse();
            response.Success = true;
            response.Message = message;
            return response;
        }

        public static ApiResponse ErrorResult(string message, string? errorCode = null)
        {
            var response = new ApiResponse();
            response.Success = false;
            response.Message = message;
            response.ErrorCode = errorCode;
            return response;
        }
    }
}
