﻿using System;
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

        /// <summary>
        /// Finds the first service that can handle the message.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static IService FindServiceForMessage(ServiceMessage message)
        {
            var firstOrDefault = Services.FirstOrDefault(s => s.CanHandleMessage(message));
            if (firstOrDefault != null)
                return firstOrDefault;
            else return null;
        }
    }
}
