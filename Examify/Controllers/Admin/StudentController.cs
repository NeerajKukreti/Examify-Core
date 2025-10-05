using DataModel;
using Examify.Services;
using Microsoft.AspNetCore.Mvc;
using Model.DTO;

namespace Examify.Controllers.Admin
{
    public class StudentController : Controller
    {
        private readonly IStudentService _studentService;
        private readonly IStateService _stateService;
        private readonly IWebHostEnvironment _env;

        public StudentController(IStudentService studentService, IStateService stateService, IWebHostEnvironment env)
        {
            _studentService = studentService;
            _stateService = stateService;
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
        public async Task<IActionResult> Create(StudentDTO model)
        {
            if (ModelState.IsValid)
            {
                model.InstituteId = HttpContext.Session.GetInt32("InstituteId") ?? 3;
                model.UserName = model.Mobile; // Username is same as mobile
                model.IsActive = true;
                
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

            // Load lookup data
            await LoadLookupData(model);

            return PartialView("_Create", model);
        }

        // POST: Admin/Student/Edit
        [HttpPost]
        public async Task<IActionResult> Edit(StudentDTO model)
        {
            if (ModelState.IsValid)
            {
                model.InstituteId = HttpContext.Session.GetInt32("InstituteId") ?? 3;
                model.UserName = model.Mobile; // Username is same as mobile
                
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
            try
            {
                var success = await _studentService.ChangeStatusAsync(id);
                
                if (success)
                {
                    return Json(new { 
                        success = true, 
                        message = "Student status updated successfully!" 
                    });
                }
                else
                {
                    return Json(new { 
                        success = false, 
                        message = "Failed to update student status. Please try again." 
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new { 
                    success = false, 
                    message = "An error occurred while updating student status." 
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

                // TODO: Load Classes and Batches when those services are ready
                model.Classes = new List<ClassDTO>();
                model.Batches = new List<BatchDTO>();
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