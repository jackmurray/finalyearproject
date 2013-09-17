using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace LibService
{
    public static class ServiceRegistration
    {
        private static List<ServiceBase> Services = new List<ServiceBase>();

        public static void Register(ServiceBase s)
        {
            Services.Add(s);
        }

        /// <summary>
        /// Finds the first service that can handle the message.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static ServiceBase FindServiceForMessage(ServiceMessage message)
        {
            var firstOrDefault = Services.FirstOrDefault(s => s.CanHandleMessage(message));
            return firstOrDefault;
        }

        public static void Start(X509Certificate2 cert, int port)
        {
            new SslServer(cert).Listen(10451);
        }
    }
}
