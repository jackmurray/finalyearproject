using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibService
{
    public static class ServiceRegistration
    {
        private static List<IService> Services = new List<IService>();

        public static void Register(IService s)
        {
            Services.Add(s);
        }

        public static IService FindServiceForMessage(byte[] message)
        {
            var firstOrDefault = Services.FirstOrDefault(s => s.CanHandleMessage(message));
            if (firstOrDefault != null)
                return firstOrDefault;
            else return null;
        }
    }
}
