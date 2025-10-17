using DataModel;
using Examify.Attributes;
using Examify.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileSystemGlobbing;
using Model.DTO;

namespace Examify.Controllers.Admin
{
    [AutoLoginAuthorize("admin", "Test@123", "Admin", "Teacher")] // Auto-login with credentials and restrict to Admin/Teacher roles
    public class StudentController : Controller
    {
        private readonly IStudentService _studentService;
        private readonly IStateService _stateService;
        private readonly IWebHostEnvironment _env;
        private readonly IBatchService _batchService;
        private readonly IClassService _classService;

        public StudentController(
            IBatchService batchService,
            IClassService classService,
            IStudentService studentService,
            IStateService stateService,
            IWebHostEnvironment env)
        {
            _studentService = studentService;
            _stateService = stateService;
            _batchService = batchService;
            _classService = classService;
            _env = env;
        }

        // GET: Admin/Student
        public IActionResult Index()
        {
            return View("Index");
        }

        // GET: Admin/Student/LoadStudents
        [HttpGet]
        public async Task<IActionResult> LoadStudents()
        {
            // Get InstituteId from session (similar to how questions work)
            var instituteId = HttpContext.Session.GetInt32("InstituteId") ?? 3; // Default to 1 for now

            var students = await _studentService.GetAllAsync(instituteId);
            return Json(new { data = students });
        }

        // GET: Admin/Student/Create
        public async Task<IActionResult> Create()
        {
            var model = new StudentDTO
            {
                StudentName = "",
                Mobile = "",
                UserName = "",
                Password = "",
                InstituteId = HttpContext.Session.GetInt32("InstituteId") ?? 1
            };

            // Load lookup data
            await LoadLookupData(model);

            return PartialView("_Create", model);
        }

        // POST: Admin/Student/Create
        [HttpPost]
        public async Task<IActionResult> Create(StudentDTO model, int? ClassId)
        {
            if (ModelState.IsValid)
            {
                model.InstituteId = HttpContext.Session.GetInt32("InstituteId") ?? 3;
                model.UserName = model.Mobile; // Username is same as mobile
                model.IsActive = true;

                // Validate that BatchId belongs to the selected ClassId (only if both are provided)
                if (ClassId.HasValue && ClassId.Value > 0 && model.BatchId.HasValue && model.BatchId.Value > 0)
                {
                    var batches = await _batchService.GetBatchesByClassIdAsync(ClassId.Value);
                    if (!batches.Any(b => b.BatchId == model.BatchId.Value))
                    {
                        return Json(new { success = false, message = "Selected batch does not belong to the selected class." });
                    }
                }

                // Validate that if ClassId is provided, BatchId should also be provided
                if (ClassId.HasValue && ClassId.Value > 0 && (!model.BatchId.HasValue || model.BatchId.Value == 0))
                {
                    return Json(new { success = false, message = "Please select a batch when class is selected." });
                }

                var success = await _studentService.CreateAsync(model);
                if (success)
                    return Json(new { success = true, message = "Student created successfully!" });
                else
                    return Json(new { success = false, message = "Failed to create student. Please try again." });
            }

            // Return validation errors
            var errors = ModelState.Where(x => x.Value.Errors.Count > 0)
                .Select(x => new { Field = x.Key, Errors = x.Value.Errors.Select(e => e.ErrorMessage) })
                .ToList();
            return Json(new { success = false, errors });
        }

        // GET: Admin/Student/Edit/{id}
        public async Task<IActionResult> Edit(int id)
        {
            var instituteId = HttpContext.Session.GetInt32("InstituteId") ?? 3;
            var student = await _studentService.GetByIdAsync(id, instituteId);

            if (student == null) return NotFound();

            // Convert to DTO for editing
            var model = new StudentDTO
            {
                StudentId = student.StudentId,
                InstituteId = student.InstituteId,
                BatchId = student.BatchId,
                StudentName = student.StudentName,
                FatherName = student.FatherName,
                DateOfBirth = student.DateOfBirth,
                Category = student.Category,
                StateId = student.StateId,
                Address = student.Address,
                City = student.City,
                Pincode = student.Pincode,
                Mobile = student.Mobile,
                SecondaryContact = student.SecondaryContact,
                ParentContact = student.ParentContact,
                Email = student.Email,
                ActivationDate = student.ActivationDate,
                Validity = student.Validity,
                UserId = student.UserId,
                UserName = student.Mobile, // Username is same as mobile
                Password = "********" // Don't show actual password
            };

            // Get BatchId from StudentBatch relationship
            var activeBatch = student.StudentBatches?.FirstOrDefault();
            if (activeBatch != null)
            {
                model.BatchId = activeBatch.BatchId;
            }

            // Load lookup data
            await LoadLookupData(model);

            return PartialView("_Create", model);
        }

        // POST: Admin/Student/Edit
        [HttpPost]
        public async Task<IActionResult> Edit(StudentDTO model, int? ClassId)
        {
            if (ModelState.IsValid)
            {
                model.InstituteId = HttpContext.Session.GetInt32("InstituteId") ?? 3;
                model.UserName = model.Mobile; // Username is same as mobile

                // Validate that BatchId belongs to the selected ClassId (only if both are provided)
                if (ClassId.HasValue && ClassId.Value > 0 && model.BatchId.HasValue && model.BatchId.Value > 0)
                {
                    var batches = await _batchService.GetBatchesByClassIdAsync(ClassId.Value);
                    if (!batches.Any(b => b.BatchId == model.BatchId.Value))
                    {
                        return Json(new { success = false, message = "Selected batch does not belong to the selected class." });
                    }
                }

                // Validate that if ClassId is provided, BatchId should also be provided
                if (ClassId.HasValue && ClassId.Value > 0 && (!model.BatchId.HasValue || model.BatchId.Value == 0))
                {
                    return Json(new { success = false, message = "Please select a batch when class is selected." });
                }

                var success = await _studentService.UpdateAsync(model);
                if (success)
                    return Json(new { success = true, message = "Student updated successfully!" });
                else
                    return Json(new { success = false, message = "Failed to update student. Please try again." });
            }

            // Return validation errors
            var errors = ModelState.Where(x => x.Value.Errors.Count > 0)
                .Select(x => new { Field = x.Key, Errors = x.Value.Errors.Select(e => e.ErrorMessage) })
                .ToList();
            return Json(new { success = false, errors });
        }

        // POST: Admin/Student/Delete/{id}
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _studentService.DeleteAsync(id);
            if (success)
                return Json(new { success = true, message = "Student deleted successfully!" });
            else
                return Json(new { success = false, message = "Failed to delete student. Please try again." });
        }

        [HttpPost]
        public async Task<IActionResult> ChangeStatus(int id)
        {
            var success = await _studentService.ChangeStatusAsync(id);

            if (success)
            {
                return Json(new
                {
                    success = true,
                    message = "Student status updated successfully!"
                });
            }
            else
            {
                return Json(new
                {
                    success = false,
                    message = "Failed to update student status. Please try again."
                });
            } 
        }

        private async Task LoadLookupData(StudentDTO model)
        {
            try
            {
                // Load states
                var states = await _stateService.GetAllAsync();
                model.States = states?.ToList();

                // Load categories (hardcoded for now, can be moved to config/database)
                model.Categories = new List<string>
                {
                    "General",
                    "OBC",
                    "SC",
                    "ST",
                    "EWS"
                };

                if (model.BatchId.HasValue && model.BatchId > 0)
                {
                    var batch = await _batchService.GetByIdAsync(model.BatchId.Value);
                    var batches = batch?.ClassId == null ? Enumerable.Empty<BatchDTO>() :
                        await _batchService.GetBatchesByClassIdAsync(batch.ClassId);

                    model.Batches = batches
                    .Select(b =>
                    {
                        b.IsCurrent = b.BatchId == model.BatchId.Value;
                        return b;
                    })
                    .ToList();
                }
                else
                {
                    model.Batches = new List<BatchDTO>();
                }

                // Load Classes for the institute
                var classes = await _classService.GetAllAsync(model.InstituteId) ?? Enumerable.Empty<ClassDTO>();

                var currentClassId = model.Batches?.FirstOrDefault(x => x.IsCurrent)?.ClassId;

                model.Classes = classes
                    .Select(c =>
                    {
                        c.IsCurrent = c.ClassId == currentClassId;
                        return c;
                    })
                    .ToList();

            }
            catch (Exception ex)
            {
                // Log exception and provide empty lists
                model.States = new List<StateModel>();
                model.Categories = new List<string>();
                model.Classes = new List<ClassDTO>();
                model.Batches = new List<BatchDTO>();
            }
        }

    }
}