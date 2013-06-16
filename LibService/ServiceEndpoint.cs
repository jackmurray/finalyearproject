using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using LibTrace;

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
            byte[] messageLenRaw = new byte[4];
            if (_s.Read(messageLenRaw, 0, 4) == 0)
            {
                Log.Verbose("Socket closed.");
                throw new SocketException();
            }
            int messageLen = BitConverter.ToInt32(messageLenRaw, 0);
            Log.Verbose("Expecting message of " + messageLen + " bytes.");
            if (messageLen < 0)
                throw new IndexOutOfRangeException("Message length must be a positive number. Got " + messageLen);

            byte[] messageBody = ReadBytes(messageLen);
            Log.Verbose("Finished reading message.");

            return new ServiceMessage(messageBody);
        }

        protected byte[] ReadBytes(int num)
        {
            byte[] buffer = new byte[num];
            int i = 0;
            while (i != num)
                i += _s.Read(buffer, i, num - i);

            return buffer;
        }

        protected void Send(ServiceMessage m)
        {
            _s.Write(m.Length);
            _s.Write(m.Data);
        }
    }
}
