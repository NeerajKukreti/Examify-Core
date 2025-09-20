using DataModel;
using Microsoft.Extensions.Options;
using Examify.Common;
using System.Text;

namespace Examify.Services
{
    public interface IInstituteService
    {
        Task<List<InstituteModel>> GetInstitute(string instituteId);
        Task<List<InstituteModel>> GetInstitute();
        Task<int> CreateInstitute(InstituteModel instituteModel);
        Task<int> DeleteInstitute(bool delete, string instituteId);
        Task<int> ActivateInstitute(bool activate, string instituteId);
        Task<InstituteChangePasswordModel> InstituteChangePassword(InstituteChangePasswordModel model);
    }

    public class InstituteService: IInstituteService
    {
        private readonly AppSettings _settings;

        public InstituteService(IOptions<AppSettings> settings)
        {
            _settings = settings.Value;
        }

        public async Task<List<InstituteModel>> GetInstitute(string InstituteId)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            parameters.Add("InstituteId", InstituteId);

            var values = new StringBuilder();

            foreach (KeyValuePair<string, string> parameter in parameters)
            {
                values.Append(parameter.Key + "=" + parameter.Value + "&");
            }

            var institutes = await HTTPClientWrapper<List<DataModel.InstituteModel>>
                .Get(_settings.Api, "InstituteApi/GetInstitute", values);

            return institutes;
        }

        public async Task<List<InstituteModel>> GetInstitute()
        {
            var institutes = await HTTPClientWrapper<List<DataModel.InstituteModel>>.Get(_settings.Api, url: "InstituteApi/GetInstitute");

            return institutes;
        }

        public async Task<int> CreateInstitute(InstituteModel instituteModel)
        {
            var x = await HTTPClientWrapper<InstituteModel>
                .PostRequest(_settings.Api, "InstituteApi/CreateInstitute", instituteModel);

            return x.InstituteId;
        }

        public async Task<int> DeleteInstitute(bool Delete, string InstituteId)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("Delete", Delete.ToString());
            parameters.Add("InstituteId", InstituteId);

            var values = new StringBuilder();

            foreach (KeyValuePair<string, string> parameter in parameters)
            {
                values.Append(parameter.Key + "=" + parameter.Value + "&");
            }

            var x = await HTTPClientWrapper<int>.PostRequest(_settings.Api, "InstituteApi/DeleteInstitute", values);

            return x;
        }

        public async Task<int> ActivateInstitute(bool Activate, string InstituteId)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("Activate", Activate.ToString());
            parameters.Add("InstituteId", InstituteId);

            var values = new StringBuilder();

            foreach (KeyValuePair<string, string> parameter in parameters)
            {
                values.Append(parameter.Key + "=" + parameter.Value + "&");
            }

            var x = await HTTPClientWrapper<int>.PostRequest(_settings.Api, "InstituteApi/ActivateInstitute", values);

            return x;
        }

        public async Task<InstituteChangePasswordModel> InstituteChangePassword(InstituteChangePasswordModel InstituteChangePasswordModel)
        {
            var x = await HTTPClientWrapper<InstituteChangePasswordModel>.PostRequest(_settings.Api, "InstituteApi/InstituteChangePassword", InstituteChangePasswordModel);

            return x;
        }
    }
}
