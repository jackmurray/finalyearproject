using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Text;

namespace LibService
{
    public abstract class ServiceClient : ServiceEndpoint
    {
        protected ServiceClient(SslStream s) : base(s)
        {
        }

        protected ServiceMessage Call(ServiceMessage m)
        {
            SendReq(m);
            return ReadResp(m.serviceID, m.operationID);
        }
    }
}
