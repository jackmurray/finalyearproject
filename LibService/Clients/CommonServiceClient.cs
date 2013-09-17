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

        public Version GetVersion()
        {
            ServiceMessage m = new ServiceMessage("CommonService", "GetVersion", null);
            ServiceMessageResponse response = Call(m);
            return JsonConvert.DeserializeObject<Version>(response.Data);
        }
    }
}
