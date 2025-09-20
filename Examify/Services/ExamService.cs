using Microsoft.Extensions.Options;
using DataModel;
using Examify.Common;
using Examify.Helpers;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Examify.Services
{
    public interface IExamService
    {
        Task<List<ExamModel>> GetExam(string ExamId);
        Task<List<ExamModel>> GetExam();
        Task<int> CreateExam(ExamModel examModel);
        Task<int> DeleteExam(bool delete, string examId);
        Task<int> ActivateExam(bool activate, string examId);
    }
    public class ExamService: IExamService
    {
        private readonly AppSettings _settings;

        public ExamService( IOptions<AppSettings> settings)
        { 
            _settings = settings.Value;
        }

        public async Task<List<ExamModel>> GetExam(string ExamId)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            parameters.Add("ExamId", ExamId);

            var values = new StringBuilder();

            foreach (KeyValuePair<string, string> parameter in parameters)
            {
                values.Append(parameter.Key + "=" + parameter.Value + "&");
            }

            var Exams = 
                await HTTPClientWrapper<List<ExamModel>>.Get(_settings.Api, "ExamApi/GetExam", values);

            return Exams;
        }

        public  async Task<List<ExamModel>> GetExam()
        {
            var Exams = await HTTPClientWrapper<List<ExamModel>>.Get(_settings.Api, url: "ExamApi/GetExam");
                 
            return Exams;
        }

        public  async Task<int> CreateExam(ExamModel ExamModel)
        {
            var x = await HTTPClientWrapper<ExamModel>.PostRequest(_settings.Api,"ExamApi/CreateExam", ExamModel);

            return x.ExamId;
        }

        public  async Task<int> DeleteExam(bool Delete, string ExamId)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("Delete", Delete.ToString());
            parameters.Add("ExamId", ExamId);

            var values = new StringBuilder();

            foreach (KeyValuePair<string, string> parameter in parameters)
            {
                values.Append(parameter.Key + "=" + parameter.Value + "&");
            }

            var x = await HTTPClientWrapper<int>.PostRequest(_settings.Api, "ExamApi/DeleteExam", values);

            return x;
        }

        public  async Task<int> ActivateExam(bool Activate, string ExamId)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("Activate", Activate.ToString());
            parameters.Add("ExamId", ExamId);

            var values = new StringBuilder();

            foreach (KeyValuePair<string, string> parameter in parameters)
            {
                values.Append(parameter.Key + "=" + parameter.Value + "&");
            }
                
            var x = await HTTPClientWrapper<int>.PostRequest(_settings.Api, "ExamApi/ActivateExam", values);

            return x;
        }
    }
}
