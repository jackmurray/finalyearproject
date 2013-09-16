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

        protected ServiceMessageResponse Call(ServiceMessage m)
        {
            SendReq(m);
            ServiceMessageResponse resp = ReadResp();
            if (resp.ResponseCode != HttpResponseCode.OK)
            {
                LibTrace.Trace.GetInstance("LibService").Error("Service error: " + resp.ResponseCode);
                throw new ServiceException("Service error", resp.ResponseCode);
            }

            return resp;
        }
    }

    public class ServiceException : Exception
    {
        public HttpResponseCode Code { get; private set; }

        public ServiceException(string msg, HttpResponseCode code) : base(msg)
        {
            this.Code = code;
        }
    }
}
