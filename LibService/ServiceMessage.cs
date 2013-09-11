using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace LibService
{
    public class ServiceMessage
    {
        /// <summary>
        /// The message payload.
        /// </summary>
        public string Data { get; set; }

        /// <summary>
        /// The ID of the service this message is for.
        /// </summary>
        public byte serviceID { get; set; }

        /// <summary>
        /// The operation within the service.
        /// </summary>
        public byte operationID { get; set; }

        public ServiceMessage(byte serviceID, byte operationID, string data)
        {
            Data = data;
            this.serviceID = serviceID;
            this.operationID = operationID;
        }

        public string Serialize()
        {
            return Data;
        }
    }
}
