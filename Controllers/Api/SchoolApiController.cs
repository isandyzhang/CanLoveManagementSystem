using Microsoft.AspNetCore.Mvc;
using CanLove_Backend.Data.Contexts;
using CanLove_Backend.Data.Models.Options;
using CanLove_Backend.Models.Api.Requests;
using Microsoft.EntityFrameworkCore;

namespace CanLove_Backend.Controllers.Api;
    [ApiController]
    [Route("api/school")]
    public class SchoolApiController : ControllerBase
    {
        private readonly CanLoveDbContext _context;

        public SchoolApiController(CanLoveDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// 取得所有學校列表（供 React 前端使用）
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetSchools()
        {
            try
            {
                var schools = await _context.Schools
                    .OrderBy(s => s.SchoolType)
                    .ThenBy(s => s.SchoolName)
                    .Select(s => new
                    {
                        s.SchoolId,
                        s.SchoolName,
                        s.SchoolType
                    })
                    .ToListAsync();

                return Ok(new { 
                    success = true, 
                    data = schools 
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    success = false, 
                    message = $"取得學校列表失敗：{ex.Message}" 
                });
            }
        }

        /// <summary>
        /// 根據類型取得學校列表（供 React 前端使用）
        /// </summary>
        [HttpGet("by-type/{schoolType}")]
        public async Task<IActionResult> GetSchoolsByType(string schoolType)
        {
            try
            {
                var schools = await _context.Schools
                    .Where(s => s.SchoolType == schoolType)
                    .OrderBy(s => s.SchoolName)
                    .Select(s => new
                    {
                        s.SchoolId,
                        s.SchoolName,
                        s.SchoolType
                    })
                    .ToListAsync();

                return Ok(new { 
                    success = true, 
                    data = schools 
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    success = false, 
                    message = $"取得學校列表失敗：{ex.Message}" 
                });
            }
        }

        /// <summary>
        /// 新增學校
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> AddSchool([FromBody] AddSchoolRequest request)
        {
            try
            {
                // 驗證輸入
                if (string.IsNullOrWhiteSpace(request.SchoolName))
                {
                    return BadRequest(new { success = false, message = "學校名稱不能為空" });
                }

                if (string.IsNullOrWhiteSpace(request.SchoolType))
                {
                    return BadRequest(new { success = false, message = "學校類型不能為空" });
                }

                // 檢查學校名稱是否已存在
                var existingSchool = await _context.Schools
                    .FirstOrDefaultAsync(s => s.SchoolName == request.SchoolName);
                
                if (existingSchool != null)
                {
                    return Conflict(new { success = false, message = "學校名稱已存在！" });
                }

                // 新增學校
                var newSchool = new School
                {
                    SchoolName = request.SchoolName.Trim(),
                    SchoolType = request.SchoolType
                };

                _context.Schools.Add(newSchool);
                await _context.SaveChangesAsync();

                return Ok(new { 
                    success = true, 
                    message = "學校新增成功！", 
                    schoolId = newSchool.SchoolId,
                    schoolName = newSchool.SchoolName,
                    schoolType = newSchool.SchoolType
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"新增學校失敗：{ex.Message}" });
            }
        }

        /// <summary>
        /// 刪除學校（內部方法，暫時不暴露給前端）
        /// </summary>
        [HttpDelete("{id}")]
        [ApiExplorerSettings(IgnoreApi = true)] // 隱藏此 API 不顯示在 Swagger 中
        public async Task<IActionResult> DeleteSchool(int id)
        {
            try
            {
                var school = await _context.Schools.FindAsync(id);
                if (school == null)
                {
                    return NotFound(new { success = false, message = "找不到指定的學校" });
                }

                // 檢查是否有個案使用此學校
                var hasCases = await _context.Cases.AnyAsync(c => c.SchoolId == id);
                if (hasCases)
                {
                    return BadRequest(new { success = false, message = "無法刪除，已有個案使用此學校" });
                }

                _context.Schools.Remove(school);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "學校刪除成功！" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"刪除學校失敗：{ex.Message}" });
            }
        }
    }


