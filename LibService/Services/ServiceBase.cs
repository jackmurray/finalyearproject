using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibService
{
    public abstract class ServiceBase
    {
        protected string Name;
        protected List<string> Operations;

        public abstract ServiceMessageResponse HandleMessage(ServiceMessage m);

        public virtual bool CanHandleMessage(ServiceMessage message)
        {
            if (message.serviceID != Name)
                return false;
            if (!Operations.Contains(message.operationID))
                return false;

            return true;
        }
    }
}
