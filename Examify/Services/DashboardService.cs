using Microsoft.Extensions.Options;
using Examify.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Web; 

namespace Examify.Services
{

    public class DashboardService
    {
        private readonly AppSettings _settings; 
        Uri host; //SET AND GET THE BASE FROM CONFIG
        NetworkCredential credential = new NetworkCredential(); //SET AND GET THE USER and PWD from CONFIG

        public DashboardService(IOptions<AppSettings> options)
        {
            host = new Uri(options.Value.Api);
            _settings = options.Value;
        }

        public string getDashBoardValues()
        {   
            string path = "DashboardAPI/GetDashboardValues";
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            return "";
        }
        
    }
}
