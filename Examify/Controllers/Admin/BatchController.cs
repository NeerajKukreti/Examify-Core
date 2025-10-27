using Examify.Attributes;
using Examify.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Model.DTO;

namespace Examify.Controllers.Admin
{
    //[AutoLoginAuthorize("admin", "admin123", "Admin", "Teacher")] // Auto-login with credentials and restrict to Admin/Teacher roles
    [Authorize(Roles = "Admin")]
    public class BatchController : Controller
    {
        private readonly IBatchService _batchService;
        private readonly IClassService _classService;

        public BatchController(IBatchService batchService, IClassService classService)
        {
            _batchService = batchService;
            _classService = classService;
        }

        // GET: Admin/Batch
        public IActionResult Index()
        {
            return View("Index");
        }

        // GET: Admin/Batch/LoadBatchesByClass/{classId}
        [HttpGet]
        public async Task<IActionResult> LoadBatchesByClass(int classId)
        {
            var batches = await _batchService.GetBatchesByClassIdAsync(classId);
            return Json(new { data = batches });
        }

    }
}