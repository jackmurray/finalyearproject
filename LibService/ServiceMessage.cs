using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibService
{
    public class ServiceMessage
    {
        /// <summary>
        /// The message payload.
        /// </summary>
        public byte[] Data { get; set; }

        /// <summary>
        /// The ID of the service this message is for.
        /// </summary>
        public byte serviceID { get; set; }

        /// <summary>
        /// The operation within the service.
        /// </summary>
        public byte operationID { get; set; }

        /// <summary>
        /// Length of the data payload.
        /// </summary>
        public int Length { get { return Data.Length; } }

        public ServiceMessage(byte serviceID, byte operationID, byte[] data)
        {
            Data = data;
            this.serviceID = serviceID;
            this.operationID = operationID;
        }
    }
}
