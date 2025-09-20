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
    public interface IInstituteLoginService
    {
        Task<InstituteLoginModel> InstituteLoginDetails(InstituteLoginModel model);
    }


    public class InstituteLoginService: IInstituteLoginService
    {
        private readonly AppSettings _settings;

        public InstituteLoginService(IOptions<AppSettings> settings)
        {
            _settings = settings.Value;
        }
        public async Task<InstituteLoginModel> InstituteLoginDetails(InstituteLoginModel InstituteLoginModel)
        {
            var x = await HTTPClientWrapper<InstituteLoginModel>
                .PostRequest(_settings.Api,"InstituteLoginApi/InstituteLoginDetails", InstituteLoginModel);

            return x;
        }
    }
}
