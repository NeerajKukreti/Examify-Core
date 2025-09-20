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
    public interface ISubjectService
    {
        Task<List<SubjectModel>> GetSubject(string subjectId);
        Task<List<SubjectModel>> GetSubject();
        Task<int> CreateSubject(SubjectModel subjectModel);
        Task<int> DeleteSubject(bool delete, string subjectId);
        Task<int> ActivateSubject(bool activate, string subjectId);
        Task<List<SubjectTopic>> GetSubjectTopic(string subjectId);
    }

    public class SubjectService : ISubjectService
    {
        private readonly AppSettings _settings;

        public SubjectService(IOptions<AppSettings> settings)
        {
            _settings = settings.Value;
        }

        public async Task<List<SubjectModel>> GetSubject(string SubjectId)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            parameters.Add("SubjectId", SubjectId);

            var values = new StringBuilder();

            foreach (KeyValuePair<string, string> parameter in parameters)
            {
                values.Append(parameter.Key + "=" + parameter.Value + "&");
            }

            var Subjects = await HTTPClientWrapper<List<DataModel.SubjectModel>>.Get(_settings.Api, "SubjectApi/GetSubject", values);

            return Subjects;
        }

        public async Task<List<SubjectModel>> GetSubject()
        {
            var Subjects = await HTTPClientWrapper<List<DataModel.SubjectModel>>.Get(_settings.Api, url: "SubjectApi/GetSubject");

            return Subjects;
        }

        public async Task<int> CreateSubject(SubjectModel SubjectModel)
        {
            var x = await HTTPClientWrapper<SubjectModel>.PostRequest(_settings.Api, "SubjectApi/CreateSubject", SubjectModel);

            return x.SubjectId;
        }

        public async Task<int> DeleteSubject(bool Delete, string SubjectId)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("Delete", Delete.ToString());
            parameters.Add("SubjectId", SubjectId);

            var values = new StringBuilder();

            foreach (KeyValuePair<string, string> parameter in parameters)
            {
                values.Append(parameter.Key + "=" + parameter.Value + "&");
            }

            var x = await HTTPClientWrapper<int>.PostRequest(_settings.Api, "SubjectApi/DeleteSubject", values);

            return x;
        }

        public async Task<int> ActivateSubject(bool Activate, string SubjectId)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("Activate", Activate.ToString());
            parameters.Add("SubjectId", SubjectId);

            var values = new StringBuilder();

            foreach (KeyValuePair<string, string> parameter in parameters)
            {
                values.Append(parameter.Key + "=" + parameter.Value + "&");
            }

            var x = await HTTPClientWrapper<int>.PostRequest(_settings.Api, "SubjectApi/ActivateSubject", values);

            return x;
        }

        public async Task<List<SubjectTopic>> GetSubjectTopic(string SubjectId)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            parameters.Add("SubjectId", SubjectId);

            var values = new StringBuilder();

            foreach (KeyValuePair<string, string> parameter in parameters)
            {
                values.Append(parameter.Key + "=" + parameter.Value + "&");
            }

            var topics = await HTTPClientWrapper<List<SubjectTopic>>.Get(_settings.Api, "SubjectApi/GetSubjectTopic", values);

            return topics;
        }
    }
}
