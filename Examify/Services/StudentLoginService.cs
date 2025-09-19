using DataModel;
using Microsoft.Extensions.Options;
using OnlineExam.Common;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace OnlineExam.Services
{
    public interface IStudentLoginService
    {
        Task<StudentLoginModel> StudentLoginDetails(StudentLoginModel model);
    }

    public class StudentLoginService: IStudentLoginService
    {
        private readonly AppSettings _settings;

        public StudentLoginService(IOptions<AppSettings> settings)
        {
            _settings = settings.Value;
        }

        public async Task<StudentLoginModel> StudentLoginDetails(StudentLoginModel StudentLoginModel)
        {
            var x = await HTTPClientWrapper<StudentLoginModel>.PostRequest(_settings.Api,"StudentLoginApi/StudentLoginDetails", StudentLoginModel);

            return x;
        }
    }
}
