using Microsoft.AspNetCore.Mvc;
using CanLove_Backend.Models.Mvc.ViewModels.Shared;
using CanLove_Backend.Services.Shared;

namespace CanLove_Backend.Controllers.Mvc
{
    /// <summary>
    /// 學校管理控制器
    /// </summary>
    public class SchoolController : Controller
    {
        private readonly SchoolService _schoolService;

        public SchoolController(SchoolService schoolService)
        {
            _schoolService = schoolService;
        }

        /// <summary>
        /// 學校列表（簡化：不含篩選）
        /// </summary>
        public async Task<IActionResult> Index()
        {
            var schools = await _schoolService.GetAllSchoolsAsync();

            var viewModel = new SchoolIndexViewModel
            {
                Schools = schools,
                AvailableSchoolTypes = new List<string>()
            };

            return View(viewModel);
        }

        /// <summary>
        /// 新增學校
        /// </summary>
        [HttpGet]
        public IActionResult Create()
        {
            var viewModel = new SchoolCreateViewModel();
            return View(viewModel);
        }

        /// <summary>
        /// 新增學校
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create(SchoolCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                var request = new CanLove_Backend.Models.Api.Requests.Shared.AddSchoolRequest
                {
                    SchoolName = model.SchoolName,
                    SchoolType = model.SchoolType
                };

                var result = await _schoolService.AddSchoolAsync(request);
                
                if (result.Success)
                {
                    TempData["SuccessMessage"] = "學校新增成功";
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError("", result.Message);
                }
            }

            return View(model);
        }



        // 刪除學校功能已暫停（僅保留新增）。
    }
}
