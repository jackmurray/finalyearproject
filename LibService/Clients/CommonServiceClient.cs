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

        public Version GetVersion()
        {
            ServiceMessage m = new ServiceMessage(CommonService.SERVICE_ID, CommonService.GET_VERSION, new byte[] {});
            ServiceMessage response = Call(m);
            byte[] buf = new byte[4];
            Buffer.BlockCopy(response.Data, 0, buf, 0, 4);
            int major = LibUtil.Util.Decode(response.Data, 0);
            Buffer.BlockCopy(response.Data, 4, buf, 0, 4);
            int minor = LibUtil.Util.Decode(buf);
            Buffer.BlockCopy(response.Data, 8, buf, 0, 4);
            int build = LibUtil.Util.Decode(buf);
            Buffer.BlockCopy(response.Data, 12, buf, 0, 4);
            int revision = LibUtil.Util.Decode(buf);

            return new Version(major, minor, build, revision);
        }
    }
}
