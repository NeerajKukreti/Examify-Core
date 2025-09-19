using DataModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
namespace OnlineExam.Controllers
{
    public class InstituteController : Controller
    {
        private readonly IFileService _fileService;
        private readonly AppSettings _settings;
        private readonly IInstituteService _instituteService ;
        private readonly IStateService _stateService;
        public InstituteController(IFileService fileService, 
            IOptions<AppSettings> settings, 
            IInstituteService instituteService, 
            IStateService stateService )
        {
            _fileService = fileService;
            _settings = settings.Value;
            _instituteService = instituteService;
            _stateService = stateService;
        }
        public async Task<ActionResult> CreateInstitute()
        {
            InstituteModel ob = new InstituteModel();

            ob.ActivationDate = DateTime.Now.AddYears(1);


            var states = await _stateService.GetState();

            ob.StateList = states.Select(x =>
            new SelectListItem
            {
                Text = x.State,
                Value = x.StateId.ToString()
            }).ToList();

            return View("CreateInstitute", ob);
        }

        [HttpPost]
        public async Task<int> CreateInstitute(InstituteModel Institute, List<IFormFile> files)
        {
            Institute.Logo = await _fileService.SaveUploadedFile(files[0]);
            return await _instituteService.CreateInstitute(Institute);
        }

        // GET: Admin
        public ActionResult Index()
        {
            return View("InstituteList");
        }



        public async Task<ActionResult> LoadInstitute()
        {
            List<InstituteModel> ob = await _instituteService.GetInstitute();

            return Json(new { data = ob });
        }

        public async Task<ActionResult> InsInstituteProfile()
        {
            InstituteLoginModel Institute = new InstituteLoginModel(); //Session["Institute"] as InstituteLoginModel;
            return await EditInstitute(Institute.StatusCode.ToString(), "Update");
        }
         
        public async Task<ActionResult> EditInstitute(string InstituteId, string CallFrom)
        {
            var serviceData = await _instituteService.GetInstitute(InstituteId);

            var institute = serviceData.FirstOrDefault();

            var states = await _stateService.GetState();

            institute.StateList = states.Select(x =>

            new SelectListItem
            {
                Text = x.State,
                Value = x.StateId.ToString()
            }).ToList();

            var uploadPath = _settings.UploadPath;

            institute.LogoPath = uploadPath + institute.Logo;
            if (true)//((Session["Institute"]) != null)
            {
                institute.CallFrom = CallFrom;
                return View("UpdateInstitute", institute);
            }
            else
            {
                return View("CreateInstitute", institute);
            }
        }

        public async Task<int> ActivateInstitute(bool Activate, string InstituteId)
        {
            return await _instituteService.ActivateInstitute(Activate, InstituteId);
        }

        public async Task<int> DeleteInstitute(bool Delete, string InstituteId)
        {
            return await _instituteService.DeleteInstitute(Delete, InstituteId);
        }

        // GET: Admin
        public ActionResult ChangePassword()
        {
            return View("ChangePassword", new InstituteChangePasswordModel());
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<int> ChangePassword(InstituteChangePasswordModel InstituteChangePassword)
        {

            InstituteChangePassword.InstituteId = 1;//SessionState.GetInstitute().StatusCode;
            InstituteChangePassword = await _instituteService.InstituteChangePassword(InstituteChangePassword);

            return InstituteChangePassword.StatusCode;

        }
    }
}