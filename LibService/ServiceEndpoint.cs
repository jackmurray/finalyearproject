using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using LibTrace;
using LibUtil;

namespace LibService
{
    public abstract class ServiceEndpoint
    {
        protected SslStream _s;
        protected Trace Log = Trace.GetInstance("LibService");

        protected ServiceEndpoint(SslStream s)
        {
            _s = s;
        }

        protected ServiceMessage Read()
        {
            byte serviceID = ReadSingleByte();
            byte operationID = ReadSingleByte();
            Log.Verbose(String.Format("Message for serviceID {0}, operation {1}", serviceID, operationID));

            byte[] messageLenRaw = ReadBytes(4);
            int messageLen = Util.Decode(messageLenRaw);
            Log.Verbose("Expecting message of " + messageLen + " bytes.");
            if (messageLen < 0)
                throw new IndexOutOfRangeException("Message length must be a positive number. Got " + messageLen);

            byte[] messageBody = ReadBytes(messageLen);
            Log.Verbose("Finished reading message.");

            return new ServiceMessage(serviceID, operationID, messageBody);
        }

        protected byte[] ReadBytes(int num)
        {
            byte[] buffer = new byte[num];
            int bytesRead = 0;
            while (bytesRead != num)
            {
                int thisCall = _s.Read(buffer, bytesRead, num - bytesRead);
                if (thisCall == 0)
                {
                    Log.Verbose("Socket closed.");
                    throw new SocketException();
                }
                bytesRead += thisCall;
            }

            return buffer;
        }

        protected byte ReadSingleByte()
        {
            byte[] buf = ReadBytes(1);
            return buf[0];
        }

        protected void Send(ServiceMessage m)
        {
            MemoryStream buffer = new MemoryStream(MessageSize(m));
            buffer.WriteByte(m.serviceID);
            buffer.WriteByte(m.operationID);
            buffer.Write(Util.Encode(m.Length), 0, sizeof (Int32));
            buffer.Write(m.Data, 0, m.Data.Length);
            _s.Write(buffer.ToArray());
        }

        protected int MessageSize(ServiceMessage m)
        {
            return sizeof (Int32) + m.Data.Length;
        }
    }
}
