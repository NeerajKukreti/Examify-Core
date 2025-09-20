 using DataModel;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Examify.Common;

namespace Examify.Services
{
    public interface IAdminLoginService
    {
        Task<AdminLoginModel> AdminLoginDetails(AdminLoginModel adminLoginModel);
    }

    public class AdminLoginService: IAdminLoginService
    {
        private readonly AppSettings _settings;

        public AdminLoginService(IOptions<AppSettings> options)
        {
            _settings = options.Value;
        }


        public async Task<AdminLoginModel> AdminLoginDetails(AdminLoginModel AdminLoginModel)
        {
            var x = await HTTPClientWrapper<AdminLoginModel>
                .PostRequest(_settings.Api , "AdminLoginApi/AdminLoginDetails", AdminLoginModel);

            return x;
        }
    }
}
