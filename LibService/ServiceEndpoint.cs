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

            string[] url = m.URL.Split(new char[] {'/'}, StringSplitOptions.RemoveEmptyEntries);
            return new ServiceMessage(byte.Parse(url[0]), byte.Parse(url[1]), m.Body);
        }

        protected ServiceMessage ReadResp(byte serviceID, byte operationID)
        {
            HttpMessage m = HttpMessage.Parse(_s);
            return new ServiceMessage(serviceID, operationID, m.Body);
        }

        protected void SendReq(ServiceMessage m)
        {
            Send(m, HttpMessageType.REQUEST, null);
        }

        protected void SendResp(ServiceMessage m, HttpResponseCode code)
        {
            Send(m, HttpMessageType.RESPONSE, code);
        }

        protected void Send(ServiceMessage m, HttpMessageType type, HttpResponseCode code)
        {
            string msg = m.Serialize();
            /*int len = msg.Length;
            MemoryStream buffer = new MemoryStream(len + sizeof(int)); //initialise buffer with all the space we need
            buffer.Write(Util.Encode(len), 0, sizeof (int)); //write the length of the message
            byte[] encodedmsg = Encoding.UTF8.GetBytes(msg);
            buffer.Write(encodedmsg, 0, encodedmsg.Length); //followed by the msg itself
            _s.Write(buffer.ToArray());*/

            HttpMessage http = new HttpMessage()
                {
                    Type = type,
                    Body = msg,
                    Method = HttpMethod.POST,
                    URL = string.Format("/{0}/{1}", m.serviceID, m.operationID),
                    Code = code
                };
            byte[] encoded = http.GetUnicodeBytes();
            _s.Write(encoded);
        }
    }
}
