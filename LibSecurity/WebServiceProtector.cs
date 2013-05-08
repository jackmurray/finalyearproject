using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel.Channels;
using System.Text;
using System.Xml;

namespace LibSecurity
{
    public class WebServiceProtector : LibUtil.IWebServiceMessageProcessor
    {
        private const string HEADER_SIGNATURE = "x-signature";
        private const string HEADER_GUID = "x-guid";
        private bool _isServer;

        public WebServiceProtector(bool isServer)
        {
            _isServer = isServer;
        }

        public bool VerifyMessage(Message m)
        {
            return true;
        }

        public Message ProtectMessage(ref Message m)
        {
            m = ProtectMessageBody(m); //Encrypt
            m = _isServer ? AddResponseSignatureHeaders(ref m) : AddRequestSignatureHeaders(ref m); //And then sign.
            return m;
        }

        //Yes, duplicating this sucks, but HttpRequest/HttpResponseMessageProperty aren't common subclasses of anything, so there's nothing we can do.
        private Message AddRequestSignatureHeaders(ref Message m)
        {
            HttpRequestMessageProperty httpRequestMessage;
            object httpRequestMessageObject;
            if (m.Properties.TryGetValue(HttpRequestMessageProperty.Name, out httpRequestMessageObject))
            {
                httpRequestMessage = httpRequestMessageObject as HttpRequestMessageProperty;
                if (string.IsNullOrEmpty(httpRequestMessage.Headers[HEADER_SIGNATURE]))
                {
                    httpRequestMessage.Headers[HEADER_SIGNATURE] = GetMessageSignature(m);
                }
                if (string.IsNullOrEmpty(httpRequestMessage.Headers[HEADER_GUID]))
                {
                    httpRequestMessage.Headers[HEADER_GUID] = GetGuid().ToString();
                }
            }
            else
            {
                httpRequestMessage = new HttpRequestMessageProperty();
                httpRequestMessage.Headers.Add(HEADER_SIGNATURE, GetMessageSignature(m));
                httpRequestMessage.Headers.Add(HEADER_GUID, GetGuid().ToString());
                m.Properties.Add(HttpRequestMessageProperty.Name, httpRequestMessage);
            }
            return m;
        }

        private Message AddResponseSignatureHeaders(ref Message m)
        {
            HttpResponseMessageProperty httpResponseMessage;
            object httpResponseMessageObject;
            if (m.Properties.TryGetValue(HttpResponseMessageProperty.Name, out httpResponseMessageObject))
            {
                httpResponseMessage = httpResponseMessageObject as HttpResponseMessageProperty;
                if (string.IsNullOrEmpty(httpResponseMessage.Headers[HEADER_SIGNATURE]))
                {
                    httpResponseMessage.Headers[HEADER_SIGNATURE] = GetMessageSignature(m);
                }
                if (string.IsNullOrEmpty(httpResponseMessage.Headers[HEADER_GUID]))
                {
                    httpResponseMessage.Headers[HEADER_GUID] = GetGuid().ToString();
                }
            }
            else
            {
                httpResponseMessage = new HttpResponseMessageProperty();
                httpResponseMessage.Headers.Add(HEADER_SIGNATURE, GetMessageSignature(m));
                httpResponseMessage.Headers.Add(HEADER_GUID, GetGuid().ToString());
                m.Properties.Add(HttpResponseMessageProperty.Name, httpResponseMessage);
            }
            return m;
        }

        private Message ProtectMessageBody(Message m)
        {
            XmlDocument doc = new XmlDocument();

            MemoryStream ms = new MemoryStream();

            XmlWriter writer = XmlWriter.Create(ms);

            m.WriteMessage(writer);

            writer.Flush();

            ms.Position = 0;

            doc.Load(ms);

            XmlNodeList list = doc.GetElementsByTagName("GetVersion");
            if (list.Count > 0)
                list[0].AppendChild(doc.CreateNode(XmlNodeType.Element, "test", "http://lol.com"));
            else
                doc.GetElementsByTagName("GetVersionResponse")[0].AppendChild(doc.CreateNode(XmlNodeType.Element, "test", "http://lol.com"));

            ms.SetLength(0);

            writer = XmlWriter.Create(ms);

            doc.WriteTo(writer);

            writer.Flush();

            ms.Position = 0;

            XmlReader reader = XmlReader.Create(ms);

            Message newmessage = Message.CreateMessage(reader, int.MaxValue, m.Version);
            newmessage.Properties.CopyProperties(m.Properties);
            return newmessage;
        }

        private string MessageBodyToString(XmlDictionaryReader r)
        {
            var ms = new MemoryStream();
            var w = XmlWriter.Create(ms, new XmlWriterSettings { Indent = true, IndentChars = "  ", OmitXmlDeclaration = true });
            //w.WriteStartElement("s", "Body", "http://schemas.xmlsoap.org/soap/envelope/");
            while (r.NodeType != XmlNodeType.EndElement && r.LocalName != "Body" && r.NamespaceURI != "http://schemas.xmlsoap.org/soap/envelope/")
            {
                if (r.NodeType != XmlNodeType.Whitespace)
                {
                    w.WriteNode(r, true);
                }
                else
                {
                    r.Read(); // ignore whitespace
                }
            }
            //w.WriteEndElement();
            w.Flush();
            var body = Encoding.UTF8.GetString(ms.ToArray());
            return body;
        }

        private string WrapBodyContents(string body)
        {
            return "<s:Body xmlns:s=\"http://schemas.xmlsoap.org/soap/envelope/\">" + body + "</s:Body>";
        }

        private string GetMessageSignature(Message m)
        {
            return "fake-signature";
        }

        private Guid GetGuid()
        {
            return Guid.NewGuid();
        }
    }
}
