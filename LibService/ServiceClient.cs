using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Text;

namespace LibService
{
    public class ServiceClient : ServiceEndpoint
    {
        public ServiceClient(SslStream s) : base(s)
        {
        }

        public ServiceMessage Call(ServiceMessage m)
        {
            Send(m);
            return Read();
        }
    }
}
