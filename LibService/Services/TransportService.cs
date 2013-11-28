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
        public delegate void JoinGroupEncryptedHandler(IPAddress ip, byte[] key, byte[] nonce);
        public delegate void JoinGroupHandler(IPAddress ip);

        private JoinGroupEncryptedHandler Handler_JoinGroupEncrypted;
        private JoinGroupHandler Handler_JoinGroup;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="c">New UdpClient object that will be used for communication. Initialise it as AF_INET but don't do anything else.</param>
        public TransportService(JoinGroupHandler Handler_JoinGroup, JoinGroupEncryptedHandler Handler_JoinGroupEncrypted)
        {
            Name = "TransportService";
            Operations = new List<string>() { "JoinGroup", "JoinGroupEncrypted" };
            this.Handler_JoinGroup = Handler_JoinGroup;
            this.Handler_JoinGroupEncrypted = Handler_JoinGroupEncrypted;
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

                case "JoinGroupEncrypted":
                    var data = JsonConvert.DeserializeObject<Tuple<string, byte[], byte[]>>(m.Data);

                    if (!IPAddress.TryParse(data.Item1, out ip))
                        return Error("Failed to parse IP.");

                    if (!LibUtil.Util.IsMulticastAddress(ip))
                        return Error("IP address was not a multicast group address.");
                    try
                    {
                        this.Handler_JoinGroupEncrypted(ip, data.Item2, data.Item3); //item2 = key, item3 = nonce
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
