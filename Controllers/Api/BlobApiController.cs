using CanLove_Backend.Services.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace CanLove_Backend.Controllers.Api
{
    [ApiController]
    [Route("api/blob")] 
    public class BlobApiController : ControllerBase
    {
        private readonly IBlobService _blobService;

        public BlobApiController(IBlobService blobService)
        {
            _blobService = blobService;
        }

        [HttpPost("upload")]
        [RequestSizeLimit(10_000_000)] // 10MB
        [DisableRequestSizeLimit]
        public async Task<IActionResult> Upload([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { success = false, message = "未收到檔案" });
            }

            var allowed = new[] { "image/jpeg", "image/png", "image/gif" };
            if (!allowed.Contains(file.ContentType))
            {
                return BadRequest(new { success = false, message = "僅支援 JPG/PNG/GIF" });
            }

            await using var stream = file.OpenReadStream();
            // 與個案大頭貼共用相同容器命名策略（CaseController 使用 "cases"）
            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            var safeName = System.IO.Path.GetFileName(file.FileName);
            var inputCaseId = (Request.Form["caseId"].FirstOrDefault() ?? Request.Query["caseId"].FirstOrDefault())?.Trim();
            var folder = string.IsNullOrWhiteSpace(inputCaseId) ? "family-trees/unknown" : $"family-trees/{inputCaseId}";
            var fileName = $"{folder}/upload_{timestamp}_{safeName}";

            var blob = await _blobService.UploadFileAsync(
                stream,
                "cases",
                fileName,
                file.ContentType,
                uploadedBy: null,
                isTemp: false);

            return Ok(new { success = true, blobId = blob.BlobId });
        }
    }
}


