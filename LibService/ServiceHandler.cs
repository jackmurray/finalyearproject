using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using System.IO;
using LibTrace;

namespace LibService
{
    public class ServiceHandler : ServiceEndpoint
    {
        public ServiceHandler(SslStream s) : base(s)
        {
        }

        public int HandleMessage()
        {
            ServiceMessage m;
            try
            {
                m = ReadReq();
                if (m == null)
                    return -1;
                Log.Verbose(String.Format("Got a message for serviceID:{0}, operationID:{1}", m.serviceID, m.operationID));
            }
            catch (SocketException)
            {
                return -1; //socket closed.
            }
            

            IService service = ServiceRegistration.FindServiceForMessage(m);
            if (service == null)
                throw new Exception("No suitable service handler found.");

            try
            {
                ServiceMessageResponse messageResponse = service.HandleMessage(m);
                SendResp(messageResponse);
                Log.Verbose("Message response sent.");
            }
            catch (Exception ex)
            {
                Log.Error("Service exception: " + ex.Message);
                SendResp(new ServiceMessageResponse(ex.Message, HttpResponseCode.INT_SRV_ERR));
                return -1;
            }
            return 0; //success
        }
    }
}
