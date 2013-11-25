using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using System.IO;
using LibTrace;
using LibConfig;

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


            ServiceBase service = ServiceRegistration.FindServiceForMessage(m);
            
            if (service == null)
                throw new Exception("No suitable service handler found.");

            if (!TrustedKeys.Contains(_s.RemoteCertificate.GetCertHashString()) && service.GetType() != typeof(PairingService))
            {
                Log.Error("Unpaired device tried to call a privileged service operation. Device fingerprint: " + _s.RemoteCertificate.GetCertHashString() + " service:operationID=" + m.serviceID + ":" + m.operationID);
                SendResp(new ServiceMessageResponse("Access Denied", HttpResponseCode.AUTH_REQ));
                return -1;
            }

            try
            {
                ServiceMessageResponse messageResponse = service.HandleMessage(m, _s.RemoteCertificate);
                SendResp(messageResponse);
                if (messageResponse.ResponseCode == HttpResponseCode.OK)
                    Log.Verbose("Message response sent.");
                else
                    Log.Error("Error in service " + m.serviceID + "/" + m.operationID + ": " + messageResponse.Data);
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
