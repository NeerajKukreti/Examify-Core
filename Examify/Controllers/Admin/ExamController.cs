using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using DataModel;
using Examify.Common;
using Examify.Helpers;
using Examify.Services;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Examify.Controllers
{
    public class ExamController : Controller
    {
        private readonly IFileService _fileService;
        private readonly AppSettings _settings;
        private readonly IExamService _examService;

        public ExamController(IFileService fileService, IOptions<AppSettings> options, IExamService examService)
        {
            _fileService = fileService;
            _settings = options.Value;
            _examService = examService;
        }
        public async Task<IActionResult> CreateExam()
        {
            ExamModel ob = new ExamModel();

            return View("CreateExam", ob);
        }

        [HttpPost]
        public async Task<int> CreateExam(ExamModel Exam, List<IFormFile> files)
        {
            Exam.Image = await _fileService.SaveUploadedFile(files[0]);
            return await _examService.CreateExam(Exam);
        }

        // GET: Admin
        public IActionResult Index()
        {
            return View("ExamList");
        }

        public async Task<IActionResult> LoadExam()
        {
            List<ExamModel> ob = await _examService.GetExam();

            return Json(new { data = ob } );
        }

        public async Task<IActionResult> EditExam(string ExamId)
        {
            var serviceData = await _examService.GetExam(ExamId);

            var exam = serviceData.FirstOrDefault();

            var uploadPath = _settings.UploadPath;

            exam.Image = uploadPath + exam.Image;

            return View("CreateExam", exam);
        }

        public async Task<int> ActivateExam(bool Activate, string ExamId)
        {
            return await _examService.ActivateExam(Activate, ExamId);
        }

        public async Task<int> DeleteExam(bool Delete, string ExamId)
        {
            return await _examService.DeleteExam(Delete, ExamId);
        }
    }
}