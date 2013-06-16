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
                m = Read();
            }
            catch (SocketException)
            {
                return -1; //socket closed.
            }
            

            IService service = ServiceRegistration.FindServiceForMessage(m);
            if (service == null)
                throw new Exception("No suitable service handler found.");

            ServiceMessage messageResponse = service.HandleMessage(m);
            Send(messageResponse);
            Log.Verbose("Message response sent.");
            return 0; //success
        }
    }
}
