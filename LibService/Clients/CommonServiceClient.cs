using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Text;

namespace LibService
{
    public class CommonServiceClient : ServiceClient
    {
        public CommonServiceClient(SslStream s) : base(s)
        {
        }

        public int GetVersion()
        {
            ServiceMessage m = new ServiceMessage(CommonService.SERVICE_ID, CommonService.GET_VERSION, new byte[] {});
            ServiceMessage response = Call(m);
            return response.Data[0];
        }
    }
}
