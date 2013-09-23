using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace LibService
{
    public abstract class ServiceBase
    {
        protected string Name;
        protected List<string> Operations;

        public abstract ServiceMessageResponse HandleMessage(ServiceMessage m, X509Certificate remoteParty);

        public virtual bool CanHandleMessage(ServiceMessage message)
        {
            if (message.serviceID != Name)
                return false;
            if (!Operations.Contains(message.operationID))
                return false;

            return true;
        }

        protected ServiceMessageResponse Error(string msg)
        {
            return new ServiceMessageResponse(msg, HttpResponseCode.INT_SRV_ERR);
        }
    }
}
