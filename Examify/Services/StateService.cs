using DataModel;
using Microsoft.Extensions.Options;
using OnlineExam.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web; 

namespace OnlineExam.Services
{
    public interface IStateService
    {
        Task<List<StateModel>> GetState();
    }

    public class StateService: IStateService
    {
        private readonly AppSettings _settings;

        public StateService(IOptions<AppSettings> settings)
        {
            _settings = settings.Value;

        }
        public async Task<List<StateModel>> GetState()
        {
            var states = await HTTPClientWrapper<List<DataModel.StateModel>>
                .Get(_settings.Api, "StateApi/GetState", new StringBuilder());
            
            return states;
        }

    }
}
