using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Examify.Common;
using Examify.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web; 

namespace Examify.Controllers
{
    public class CkController : Controller
    {
        private readonly AppSettings _settings;
        private readonly IFileService _fileService;
        public CkController(IFileService fileService, IOptions<AppSettings> settings)
        {
            _fileService = fileService;
            _settings = settings.Value;
        }

        // GET: Ck
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult filebrowser()
        {
            return View();
        }

        public ActionResult ImageFileBrowser(string type)
        {
            string[] filePaths = Directory.GetFiles(_settings.CkUpload);
            return View();
        }
        public ActionResult filebrowserUpload(string command, string type)
        {
            return View();
        }
        public ActionResult filebrowserImageUploadUrl(string command, string type)
        {
            return View();
        }

        public ActionResult browserServer(string command, string type, string responseType)
        {
            return View();
        }
    }
}