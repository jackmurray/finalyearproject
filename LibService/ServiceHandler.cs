using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Text;
using System.IO;
using LibTrace;

namespace LibService
{
    public class ServiceHandler
    {
        private SslStream _s;
        private Trace Log = Trace.GetInstance("ServiceHandler");

        public ServiceHandler(SslStream s)
        {
            _s = s;
        }

        public int HandleMessage()
        {
            byte[] messageLenRaw = new byte[4];
            if (_s.Read(messageLenRaw, 0, 4) == 0)
            {
                Log.Verbose("Socket closed.");
                return -1; //socket closed.
            }
            int messageLen = BitConverter.ToInt32(messageLenRaw, 0);
            Log.Verbose("Expecting message of " + messageLen + " bytes.");
            if (messageLen < 0)
                throw new IndexOutOfRangeException("Message length must be a positive number. Got " + messageLen);

            byte[] messageBody = ReadBytes(messageLen);
            Log.Verbose("Finished reading message.");

            IService service = ServiceRegistration.FindServiceForMessage(messageBody);
            if (service == null)
                throw new Exception("No suitable service handler found.");

            byte[] messageResponse = service.HandleMessage(messageBody);
            SendBytes(messageResponse);
            Log.Verbose("Message response sent.");
            return 0; //success
        }

        private byte[] ReadBytes(int num)
        {
            byte[] buffer = new byte[num];
            int i = 0;
            while (i != num)
                i += _s.Read(buffer, i, num-i); //read 1k chunks until we get it all.

            return buffer;
        }

        private void SendBytes(byte[] buffer)
        {
            _s.Write(buffer);
        }
    }
}
