using DataModel;
using Microsoft.Extensions.Options;
using Examify.Common;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Examify.Services
{
    public interface IStudentService
    {
        Task<List<StudentModel>> GetStudent(string studentId);
        Task<List<StudentModel>> GetStudent();
        Task<int> CreateStudent(StudentModel model);
        Task<int> DeleteStudent(bool delete, string studentId);
        Task<int> ActivateStudent(bool activate, string studentId);
        Task<StudentChangePasswordModel> StudentChangePassword(StudentChangePasswordModel model);
    }

    public class StudentService: IStudentService
    {
        private readonly AppSettings _settings;

        public StudentService(IOptions<AppSettings> settings)
        {
            _settings = settings.Value;
        }  

        public  async Task<List<StudentModel>> GetStudent(string StudentId)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            parameters.Add("StudentId", StudentId);

            var values = new StringBuilder();

            foreach (KeyValuePair<string, string> parameter in parameters)
            {
                values.Append(parameter.Key + "=" + parameter.Value + "&");
            }

            var Students = await HTTPClientWrapper<List<DataModel.StudentModel>>
                .Get(_settings.Api,"StudentApi/GetStudent", values);

            return Students;
        }

        public  async Task<List<StudentModel>> GetStudent()
        {
            var Students = await HTTPClientWrapper<List<DataModel.StudentModel>>.Get(_settings.Api,url: "StudentApi/GetStudent");
                 
            return Students;
        }

        public  async Task<int> CreateStudent(StudentModel StudentModel)
        {
            var x = await HTTPClientWrapper<StudentModel>.PostRequest(_settings.Api, "StudentApi/CreateStudent", StudentModel);

            return x.StudentId;
        }

        public  async Task<int> DeleteStudent(bool Delete, string StudentId)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("Delete", Delete.ToString());
            parameters.Add("StudentId", StudentId);

            var values = new StringBuilder();

            foreach (KeyValuePair<string, string> parameter in parameters)
            {
                values.Append(parameter.Key + "=" + parameter.Value + "&");
            }

            var x = await HTTPClientWrapper<int>.PostRequest(_settings.Api, "StudentApi/DeleteStudent", values);

            return x;
        }

        public  async Task<int> ActivateStudent(bool Activate, string StudentId)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("Activate", Activate.ToString());
            parameters.Add("StudentId", StudentId);

            var values = new StringBuilder();

            foreach (KeyValuePair<string, string> parameter in parameters)
            {
                values.Append(parameter.Key + "=" + parameter.Value + "&");
            }

            var x = await HTTPClientWrapper<int>.PostRequest(_settings.Api, "StudentApi/ActivateStudent", values);

            return x;
        }

        public  async Task<StudentChangePasswordModel> StudentChangePassword(StudentChangePasswordModel StudentChangePasswordModel)
        {
            var x = await HTTPClientWrapper<StudentChangePasswordModel>.PostRequest(_settings.Api, "StudentApi/StudentChangePassword", StudentChangePasswordModel);

            return x;
        }

    }
}
