using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using LibTrace;
using LibUtil;
using Newtonsoft.Json;

namespace LibService
{
    public abstract class ServiceEndpoint
    {
        protected SslStream _s;
        protected Trace Log = Trace.GetInstance("LibService");

        protected ServiceEndpoint(SslStream s)
        {
            _s = s;
        }

        protected ServiceMessage ReadReq()
        {
            HttpMessage m = HttpMessage.Parse(_s);
            if (m == null)
                return null;

            string[] url = m.URL.Split(new char[] {'/'}, StringSplitOptions.RemoveEmptyEntries);
            return new ServiceMessage(url[0], url[1], m.Body);
        }

        protected ServiceMessageResponse ReadResp()
        {
            HttpMessage m = HttpMessage.Parse(_s);
            return new ServiceMessageResponse(m.Body, m.Code);
        }

        protected void SendReq(ServiceMessage m)
        {
            string msg = m.Data;
            HttpMessage http = new HttpMessage()
            {
                Type = HttpMessageType.REQUEST,
                Body = msg,
                Method = HttpMethod.POST,
                URL = string.Format("/{0}/{1}", m.serviceID, m.operationID)
            };

            byte[] encoded = http.GetUnicodeBytes();
            _s.Write(encoded);
        }

        protected void SendResp(ServiceMessageResponse m)
        {
            string msg = m.Data;
            HttpMessage http = new HttpMessage()
            {
                Type = HttpMessageType.RESPONSE,
                Body = msg,
                Code = m.ResponseCode
            };
            byte[] encoded = http.GetUnicodeBytes();
            _s.Write(encoded);
        }
    }
}
