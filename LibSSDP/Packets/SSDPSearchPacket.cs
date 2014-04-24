using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibSSDP
{
    public class SSDPSearchPacket : SSDPPacket
    {
        public SSDPSearchPacket()
        {
            Method = LibSSDP.Method.Search;
        }
        protected override string GetSpecificHeaders()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Man: ssdp:discover\r\n");
            sb.AppendFormat("ST: {0}\r\n", ServiceType);

            return sb.ToString();
        }

        protected override void TryParseExtendedHeader(string name, string val)
        {
            return; //search doesn't have anything special
        }

        public static SSDPSearchPacket Build()
        {
            SSDPSearchPacket packet = new SSDPSearchPacket();
            return packet;
        }
    }
}
