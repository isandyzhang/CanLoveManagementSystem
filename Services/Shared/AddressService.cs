using CanLove_Backend.Data.Contexts;
using CanLove_Backend.Data.Models.Options;
using CanLove_Backend.Models.Api.Responses;
using CanLove_Backend.Models.Api.Responses.Shared;
using Microsoft.EntityFrameworkCore;

namespace CanLove_Backend.Services.Shared
{
    /// <summary>
    /// 地址服務類別
    /// </summary>
    public class AddressService
    {
        private readonly CanLoveDbContext _context;

        public AddressService(CanLoveDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// 取得所有城市
        /// </summary>
        public async Task<List<City>> GetCitiesAsync()
        {
            return await _context.Cities
                .OrderBy(c => c.CityName)
                .ToListAsync();
        }

        /// <summary>
        /// 取得指定城市的所有區域
        /// </summary>
        public async Task<List<District>> GetDistrictsByCityIdAsync(int cityId)
        {
            return await _context.Districts
                .Where(d => d.CityId == cityId)
                .OrderBy(d => d.DistrictName)
                .ToListAsync();
        }

        /// <summary>
        /// 取得所有城市（API 用）
        /// </summary>
        public async Task<ApiResponse<List<CityResponse>>> GetCitiesForApiAsync()
        {
            try
            {
                var cities = await GetCitiesAsync();
                var cityResponses = cities.Select(c => new CityResponse
                {
                    CityId = c.CityId,
                    CityName = c.CityName
                }).ToList();

                return ApiResponse<List<CityResponse>>.SuccessResponse(cityResponses, "取得城市列表成功");
            }
            catch (Exception ex)
            {
                return ApiResponse<List<CityResponse>>.ErrorResponse($"取得城市列表失敗：{ex.Message}");
            }
        }

        /// <summary>
        /// 取得指定城市的所有區域（API 用）
        /// </summary>
        public async Task<ApiResponse<List<DistrictResponse>>> GetDistrictsForApiAsync(int cityId)
        {
            try
            {
                var districts = await GetDistrictsByCityIdAsync(cityId);
                var districtResponses = districts.Select(d => new DistrictResponse
                {
                    DistrictId = d.DistrictId,
                    DistrictName = d.DistrictName,
                    CityId = d.CityId
                }).ToList();

                return ApiResponse<List<DistrictResponse>>.SuccessResponse(districtResponses, "取得區域列表成功");
            }
            catch (Exception ex)
            {
                return ApiResponse<List<DistrictResponse>>.ErrorResponse($"取得區域列表失敗：{ex.Message}");
            }
        }
    }

}
