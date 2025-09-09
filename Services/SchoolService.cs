using CanLove_Backend.Data.Contexts;
using CanLove_Backend.Data.Models.Options;
using Microsoft.EntityFrameworkCore;

namespace CanLove_Backend.Services
{
    /// <summary>
    /// 學校服務類別
    /// </summary>
    public class SchoolService
    {
        private readonly CanLoveDbContext _context;

        public SchoolService(CanLoveDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// 取得所有學校列表（按名稱排序）
        /// </summary>
        public async Task<List<School>> GetAllSchoolsAsync()
        {
            return await _context.Schools
                .OrderBy(s => s.SchoolName)
                .ToListAsync();
        }

        /// <summary>
        /// 取得所有學校列表（按類型再按名稱排序）
        /// </summary>
        public async Task<List<School>> GetAllSchoolsOrderedByTypeAsync()
        {
            return await _context.Schools
                .OrderBy(s => s.SchoolType)
                .ThenBy(s => s.SchoolName)
                .ToListAsync();
        }

        /// <summary>
        /// 根據類型取得學校列表
        /// </summary>
        public async Task<List<School>> GetSchoolsByTypeAsync(string schoolType)
        {
            return await _context.Schools
                .Where(s => s.SchoolType == schoolType)
                .OrderBy(s => s.SchoolName)
                .ToListAsync();
        }

        /// <summary>
        /// 根據 ID 取得學校
        /// </summary>
        public async Task<School?> GetSchoolByIdAsync(int schoolId)
        {
            return await _context.Schools
                .FirstOrDefaultAsync(s => s.SchoolId == schoolId);
        }

        /// <summary>
        /// 檢查學校名稱是否已存在
        /// </summary>
        public async Task<bool> SchoolNameExistsAsync(string schoolName)
        {
            return await _context.Schools
                .AnyAsync(s => s.SchoolName == schoolName);
        }

        /// <summary>
        /// 新增學校
        /// </summary>
        public async Task<School> AddSchoolAsync(string schoolName, string schoolType)
        {
            var school = new School
            {
                SchoolName = schoolName,
                SchoolType = schoolType
            };

            _context.Schools.Add(school);
            await _context.SaveChangesAsync();
            
            return school;
        }

        /// <summary>
        /// 取得學校統計資訊
        /// </summary>
        public async Task<object> GetSchoolStatisticsAsync()
        {
            var totalSchools = await _context.Schools.CountAsync();
            var schoolsByType = await _context.Schools
                .GroupBy(s => s.SchoolType)
                .Select(g => new { Type = g.Key, Count = g.Count() })
                .ToListAsync();

            return new
            {
                TotalSchools = totalSchools,
                SchoolsByType = schoolsByType
            };
        }

        /// <summary>
        /// 搜尋學校（模糊搜尋）
        /// </summary>
        public async Task<List<School>> SearchSchoolsAsync(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return await GetAllSchoolsAsync();
            }

            return await _context.Schools
                .Where(s => s.SchoolName.Contains(keyword))
                .OrderBy(s => s.SchoolName)
                .ToListAsync();
        }

        /// <summary>
        /// 檢查學校是否被個案使用
        /// </summary>
        public async Task<bool> IsSchoolInUseAsync(int schoolId)
        {
            return await _context.Cases.AnyAsync(c => c.SchoolId == schoolId);
        }

        /// <summary>
        /// 取得學校類型列表
        /// </summary>
        public async Task<List<string>> GetSchoolTypesAsync()
        {
            return await _context.Schools
                .Select(s => s.SchoolType)
                .Distinct()
                .OrderBy(t => t)
                .ToListAsync();
        }
    }
}
