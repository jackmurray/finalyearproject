using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Newtonsoft.Json;

namespace LibService
{
    public class TransportService : ServiceBase
    {
        public delegate void SetEncryptionKeyHandler(byte[] key, byte[] nonce);
        public delegate void JoinGroupHandler(IPAddress ip);
        public delegate void SetControllerAddress(IPAddress ip, ushort port);

        private SetEncryptionKeyHandler Handler_SetEncryptionKey;
        private JoinGroupHandler Handler_JoinGroup;
        private SetControllerAddress Handler_SetControllerAddress;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="c">New UdpClient object that will be used for communication. Initialise it as AF_INET but don't do anything else.</param>
        public TransportService(JoinGroupHandler Handler_JoinGroup, SetEncryptionKeyHandler Handler_SetEncryptionKey, SetControllerAddress Handler_SetControllerAddress)
        {
            Name = "TransportService";
            Operations = new List<string>() { "JoinGroup", "SetEncryptionKey", "SetControllerAddress" };
            this.Handler_JoinGroup = Handler_JoinGroup;
            this.Handler_SetEncryptionKey = Handler_SetEncryptionKey;
            this.Handler_SetControllerAddress = Handler_SetControllerAddress;
        }

        public override ServiceMessageResponse HandleMessage(ServiceMessage m, X509Certificate remoteParty)
        {
            IPAddress ip;
            switch (m.operationID)
            {
                case "JoinGroup":
                    if (!IPAddress.TryParse(m.Data, out ip))
                        return Error("Failed to parse IP.");

                    if (!LibUtil.Util.IsMulticastAddress(ip))
                        return Error("IP address was not a multicast group address.");
                    try
                    {
                        this.Handler_JoinGroup(ip);
                        return Success();
                    }
                    catch (Exception ex)
                    {
                        return Error(ex.Message);
                    }
                    break;

                case "SetEncryptionKey":
                    var data = JsonConvert.DeserializeObject<Tuple<byte[], byte[]>>(m.Data);

                    try
                    {
                        this.Handler_SetEncryptionKey(data.Item1, data.Item2); //item1 = key, item2 = nonce
                        return Success();
                    }
                    catch (Exception ex)
                    {
                        return Error(ex.Message);
                    }
                    break;

                    //Used to tell the receiver what the service address of the controller is, so it can call in for key rotation etc.
                case "SetControllerAddress":
                    var data_sca = JsonConvert.DeserializeObject<Tuple<string, ushort>>(m.Data);

                    if (!IPAddress.TryParse(data_sca.Item1, out ip))
                        return Error("Failed to parse IP.");

                    try
                    {
                        this.Handler_SetControllerAddress(ip, data_sca.Item2);
                        return Success();
                    }
                    catch (Exception ex)
                    {
                        return Error(ex.Message);
                    }
                    break;

                default:
                    throw new ArgumentException("Invalid message received.");
            }
        }
    }
}
