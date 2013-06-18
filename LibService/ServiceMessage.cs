using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibService
{
    public class ServiceMessage
    {
        public byte[] Data { get; set; }

        public int Length { get { return Data.Length; } }

        public ServiceMessage(byte[] data)
        {
            Data = data;
        }
    }
}
