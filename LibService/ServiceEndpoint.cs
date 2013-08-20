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
using Newtonsoft.Json;

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
            byte[] messageLenRaw = ReadBytes(sizeof(int));
            int messageLen = Util.Decode(messageLenRaw);
            Log.Verbose("Expecting message of " + messageLen + " bytes.");

            if (messageLen < 0)
                throw new IndexOutOfRangeException("Message length must be a positive number. Got " + messageLen);

            byte[] messageBody = ReadBytes(messageLen);
            Log.Verbose("Finished reading message.");
            string message = Encoding.UTF8.GetString(messageBody);

            return JsonConvert.DeserializeObject<ServiceMessage>(message);
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
            string msg = m.Serialize();
            int len = msg.Length;
            MemoryStream buffer = new MemoryStream(len + sizeof(int)); //initialise buffer with all the space we need
            buffer.Write(Util.Encode(len), 0, sizeof (int)); //write the length of the message
            byte[] encodedmsg = Encoding.UTF8.GetBytes(msg);
            buffer.Write(encodedmsg, 0, encodedmsg.Length); //followed by the msg itself
            _s.Write(buffer.ToArray());
        }
    }
}
