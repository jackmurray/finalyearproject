﻿using System;
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
                Log.Verbose(String.Format("Got a message for serviceID:{0}, operationID:{1}", m.serviceID, m.operationID));
            }
            catch (SocketException)
            {
                return -1; //socket closed.
            }
            

            IService service = ServiceRegistration.FindServiceForMessage(m);
            if (service == null)
                throw new Exception("No suitable service handler found.");
            Log.Verbose("Message will be handled by service: " + service.Name);
            try
            {
                ServiceMessage messageResponse = service.HandleMessage(m);
                Send(messageResponse);
                Log.Verbose("Message response sent.");
            }
            catch (SocketException ex)
            {
                Log.Error("Socket exception: " + ex.Message);
                return -1;
            }
            return 0; //success
        }
    }
}