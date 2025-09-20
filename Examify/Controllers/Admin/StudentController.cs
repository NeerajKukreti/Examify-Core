using DataModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using Examify.Common;
using Examify.Helpers;
using Examify.Services;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web; 

namespace Examify.Controllers
{
    public class StudentController : Controller
    {
        private readonly AppSettings _settings;
        private readonly IFileService _fileService;
        private readonly IStudentService _studentService;
        private readonly IStateService _stateService;

        public StudentController(IFileService fileService, 
            IOptions<AppSettings> settings, 
            IStudentService studentService,
            IStateService StateService)
        {
            _fileService = fileService;
            _settings = settings.Value;
            _studentService = studentService;
            _stateService = StateService;
        }

        public async Task<ActionResult> CreateStudent()
        {
            StudentModel ob = new StudentModel();

            var category = _settings.CasteCategory;

            //var categorylist = category.Split(',');
            //ob.CategoryList = categorylist.Select(x => new SelectListItem { Value = x, Text = x }).ToList();

            //var states = await _stateService.GetState();

            //ob.StateList = states.Select(x =>
            //new SelectListItem
            //{
            //    Text = x.State,
            //    Value = x.StateId.ToString()
            //}).ToList();

            return View("CreateStudent", ob);
        }

        [HttpPost]
        public async Task<int> CreateStudent(StudentModel Student, List<IFormFile> files)
        {
            Student.Email = await _fileService.SaveUploadedFile(files[0]);
            return await _studentService.CreateStudent(Student);
        }

        // GET: Admin
        public ActionResult Index()
        {
            return View("StudentList");
        }

        public async Task<ActionResult> LoadStudent()
        {
            List<StudentModel> ob = await _studentService.GetStudent();

            return Json(new { data = ob });
        }

        public async Task<ActionResult> StStudentProfile()
        {
            StudentLoginModel Student = new StudentLoginModel();//Session["Student"] as StudentLoginModel;
            return await EditStudent(Student.StatusCode.ToString(), "Update");
        }
         
        public async Task<ActionResult> EditStudent(string StudentId, string CallFrom)
        {
            var serviceData = await _studentService.GetStudent(StudentId);

            var Student = serviceData.FirstOrDefault();

            //var category =  _settings.CasteCategory;
            //var categorylist = category.Split(',');
            //Student.CategoryList = categorylist.Select(x => new SelectListItem { Value = x, Text = x }).ToList();

            //var states = await _stateService.GetState();

            //Student.StateList = states.Select(x =>
            
            //new SelectListItem
            //{
            //    Text = x.State,
            //    Value = x.StateId.ToString()
            //}).ToList();

            //var uploadPath = _settings.UploadPath;

            //Student.ImagePath = uploadPath + Student.Image;
            //if (true)//((Session["Student"]) != null)
            //{
            //    Student.CallFrom = CallFrom;
            //    return View("UpdateStudent", Student);
            //}
            //else
            //{
                return View("CreateStudent", Student);
            //}

            
        }

        

        public async Task<int> ActivateStudent(bool Activate, string StudentId)
        {
            return await _studentService.ActivateStudent(Activate, StudentId);
        }

        public async Task<int> DeleteStudent(bool Delete, string StudentId)
        {
            return await _studentService.DeleteStudent(Delete, StudentId);
        }

        // GET: Admin
        public ActionResult ChangePassword()
        {
            return View("ChangePassword", new StudentChangePasswordModel());
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<int> ChangePassword(StudentChangePasswordModel StudentChangePassword)
        {
            StudentChangePassword.StudentId = 1;//SessionState.GetStudent().StatusCode;
            StudentChangePassword = await _studentService.StudentChangePassword(StudentChangePassword);

            return StudentChangePassword.StatusCode;
        }


    }
}