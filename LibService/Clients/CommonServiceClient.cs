using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Text;
using Newtonsoft.Json;

namespace LibService
{
    public class CommonServiceClient : ServiceClient
    {
        public CommonServiceClient(SslStream s) : base(s)
        {
        }

        public Dictionary<string, Version> GetVersions()
        {
            ServiceMessage m = new ServiceMessage("CommonService", "GetVersions", null);
            ServiceMessageResponse response = Call(m);
            return JsonConvert.DeserializeObject<Dictionary<string,Version>>(response.Data);
        }
    }
}
