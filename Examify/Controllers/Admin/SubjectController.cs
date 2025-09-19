using DataModel;
using Microsoft.Extensions.Options;
using OnlineExam.Common;
using OnlineExam.Helpers;
using OnlineExam.Services;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace OnlineExam.Controllers
{
    public class SubjectController : Controller
    {
        private readonly AppSettings _settings;
        private readonly IFileService _fileService; private readonly ISubjectService _subjectService;

        public SubjectController(IFileService fileService,
            IOptions<AppSettings> settings,
            ISubjectService SubjectService)
        {
            _fileService = fileService;
            _settings = settings.Value;
            _subjectService = SubjectService;
        }

        public async Task<ActionResult> CreateSubject()
        {
            SubjectModel ob = new SubjectModel();

            var topics = _settings.SubjectTopics;

            var topicList = topics.Split(',');

            ob.TopicList = topicList.Select(x => new SelectListItem { Value = x, Text = x }).ToList();

            return View(ob);
        }

        [HttpPost]
        public async Task<int> CreateSubject(SubjectModel Subject, List<IFormFile> files)
        {
            Subject.Image = await _fileService.SaveUploadedFile(files[0]);
            return await _subjectService.CreateSubject(Subject);
        }

        // GET: Admin
        public ActionResult Index()
        {
            return View("SubjectList");
        }

        public async Task<IActionResult> LoadSubject()
        {
            List<SubjectModel> ob = await _subjectService.GetSubject();

            return Json(new { data = ob });
        }

        public async Task<ActionResult> EditSubject(string SubjectId)
        {
            var serviceData = await _subjectService.GetSubject(SubjectId);

            var Subject = serviceData.FirstOrDefault();

            var topics = _settings.SubjectTopics;

            var topicList = topics.Split(',');

            Subject.TopicList = topicList.Select(x => new SelectListItem { Value = x, Text = x }).ToList();

            Subject.TopicList.ForEach(x =>
            {
                x.Selected = Subject.Topics.Contains(x.Value);
            });

            var uploadPath = _settings.UploadPath;

            Subject.ImagePath = uploadPath + Subject.Image;

            return View("CreateSubject", Subject);
        }

        public async Task<int> ActivateSubject(bool Activate, string SubjectId)
        {
            return await _subjectService.ActivateSubject(Activate, SubjectId);
        }

        public async Task<int> DeleteSubject(bool Delete, string SubjectId)
        {
            return await _subjectService.DeleteSubject(Delete, SubjectId);
        }
    }
}