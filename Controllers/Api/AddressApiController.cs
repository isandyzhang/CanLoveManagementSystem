using Microsoft.AspNetCore.Mvc;
using CanLove_Backend.Data.Contexts;
using Microsoft.EntityFrameworkCore;

namespace CanLove_Backend.Controllers.Api;

[ApiController]
[Route("api/address")]
public class AddressApiController : ControllerBase
{
    private readonly CanLoveDbContext _context;

    public AddressApiController(CanLoveDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// 取得所有城市列表
    /// </summary>
    [HttpGet("cities")]
    public async Task<IActionResult> GetCities()
    {
        var cities = await _context.Cities
            .Select(c => new { 
                CityId = c.CityId, 
                CityName = c.CityName 
            })
            .OrderBy(c => c.CityId)
            .ToListAsync();
        
        return Ok(cities);
    }

    /// <summary>
    /// 根據城市ID取得區域列表
    /// </summary>
    [HttpGet("districts/{cityId}")]
    public async Task<IActionResult> GetDistrictsByCity(int cityId)
    {
        var districts = await _context.Districts
            .Where(d => d.CityId == cityId)
            .Select(d => new { 
                DistrictId = d.DistrictId, 
                DistrictName = d.DistrictName 
            })
            .OrderBy(d => d.DistrictName)
            .ToListAsync();
        
        return Ok(districts);
    }

    /// <summary>
    /// 取得所有區域列表（用於初始化）
    /// </summary>
    [HttpGet("districts")]
    public async Task<IActionResult> GetAllDistricts()
    {
        var districts = await _context.Districts
            .Select(d => new { 
                DistrictId = d.DistrictId, 
                DistrictName = d.DistrictName,
                CityId = d.CityId
            })
            .OrderBy(d => d.CityId)
            .ThenBy(d => d.DistrictName)
            .ToListAsync();
        
        return Ok(districts);
    }
}
