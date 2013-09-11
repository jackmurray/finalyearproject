using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LibService
{
    public class HttpMessage
    {
        private static readonly byte[] CRLFCRLF = new byte[] {(byte) '\r', (byte) '\n', (byte) '\r', (byte) '\n'};

        public HttpMessageType Type { get; set; }

        /// <summary>
        /// HTTP response code. Has no effect when Type is set to REQUEST.
        /// </summary>
        public HttpResponseCode Code { get; set; }
        public string URL { get; set; }
        /// <summary>
        /// HTTP headers to add. Content-Length should not be specified, as it will be added automatically from the Body property's length.
        /// </summary>
        public Dictionary<string, string> Headers = new Dictionary<string, string>();
        public HttpMethod Method { get; set; }
        public string Body { get; set; }

        public string Serialize()
        {
            StringBuilder buffer = new StringBuilder();
            if (Type == HttpMessageType.REQUEST)
                buffer.AppendFormat("{0} {1} HTTP/1.0\r\n", Method, URL);
            else
                buffer.AppendFormat("HTTP/1.0 {0}\r\n", Code);

            if (Headers.ContainsKey("Content-Length"))
            {
                if (!string.IsNullOrEmpty(Body))
                    Headers["Content-Length"] = Body.Length.ToString();
                else
                    Headers.Remove("Content-Length");
            }
            else if (!string.IsNullOrEmpty(Body))
                Headers.Add("Content-Length", Body.Length.ToString());

            foreach (KeyValuePair<string, string> kvp in Headers)
                buffer.AppendFormat("{0}: {1}\r\n", kvp.Key, kvp.Value);
            buffer.Append("\r\n");

            if (!string.IsNullOrEmpty(Body))
                buffer.Append(Body);

            return buffer.ToString();
        }

        public static HttpMessage Parse(string s)
        {
            HttpMessage m = ParseHeaders(s);

            string body = s.Substring(FindEndOfHeaders(s) + 4);
            m.Body = body;

            return m;
        }

        private static int FindEndOfHeaders(string s)
        {
            if (string.IsNullOrEmpty(s))
                throw new Exception("Invalid input string. Was null or empty.");

            return s.IndexOf("\r\n\r\n", StringComparison.Ordinal);
        }

        /// <summary>
        /// Reads just the HTTP headers from the given string. The string will be parsed until a double-CRLF is found, or the end of the string is reached.
        /// </summary>
        /// <param name="s"></param>
        public static HttpMessage ParseHeaders(string s)
        {
            HttpMessage m = new HttpMessage();
            int endOfHeaders = FindEndOfHeaders(s);
            string justheaders = endOfHeaders != -1 ? s.Substring(0, endOfHeaders) : s;

            string[] headers = justheaders.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            if (headers.Length < 1)
                throw new FormatException("No headers found.");
            string[] firstline = headers[0].Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
            if (firstline.Length < 1)
                throw new FormatException("Invalid format of first header line.");

            if (firstline[0] == "HTTP/1.0") //this is a response
            {
                m.Type = HttpMessageType.RESPONSE;
                int code = int.Parse(firstline[1]);
                if (code == HttpResponseCode.OK.Code)
                    m.Code = HttpResponseCode.OK;
                else if (code == HttpResponseCode.NOT_FOUND.Code)
                    m.Code = HttpResponseCode.NOT_FOUND;
                else if (code == HttpResponseCode.INT_SRV_ERR.Code)
                    m.Code = HttpResponseCode.INT_SRV_ERR;
                else throw new FormatException("Unsupported response code.");
            }
            else
            {
                if (firstline.Length != 3)
                    throw new FormatException("Incorrect number of space-delimeted segments in first line of message. There must be 3, got '" + firstline.Length + "'.");

                if (firstline[0] == HttpMethod.GET.ToString())
                    m.Method = HttpMethod.GET;
                else if (firstline[0] == HttpMethod.POST.ToString())
                    m.Method = HttpMethod.POST;
                else
                    throw new FormatException("Only GET and POST supported currently. Got '" + firstline[0] + "' instead.");

                m.URL = firstline[1];

                if (firstline[2] != "HTTP/1.0")
                    throw new FormatException("Last component of first header line must be HTTP/1.0. Got '" +
                                              firstline[2] + "' instead.");
            }

            for (int i = 1; i < headers.Length; i++)
            {
                int delim = headers[i].IndexOf(':');
                m.Headers.Add(headers[i].Substring(0, delim), headers[i].Substring(delim + 1));
            }

            return m;
        }

        public static HttpMessage Parse(Stream s)
        {
            /*
             * The basic idea here is that we allocate a buffer and start reading from the stream into it.
             * After each read we search for \r\n\r\n, which marks the end of the headers. Once we've got the headers we can then
             * parse them, and find the Content-Length (if present). We then read the body of the message based on Content-Length.
             */

            HttpMessage m;
            byte[] buffer = new byte[1024];
            MemoryStream ms = new MemoryStream();

            int i = 0;
            while (true)
            {
                int bytesRead = s.Read(buffer, 0, 1024);
                if (bytesRead == 0) break;

                ms.Write(buffer, 0, bytesRead); //Add what we just read to the full buffer;
                i = FindByteSequence(CRLFCRLF, buffer, 0);
                if (i != -1) break;
            }

            m = HttpMessage.ParseHeaders(Encoding.UTF8.GetString(ms.ToArray(), 0, i)); //only read up as far as the double-CRLF. anything after that may not be fully received yet.
            if (m.Headers.ContainsKey("Content-Length"))
            {
                long contentlen = long.Parse(m.Headers["Content-Length"]);
                if (contentlen > Int32.MaxValue)
                    throw new Exception("Content-Length too long. Got " + contentlen);
                i += 4;
                long alreadyhave = ms.Length - i; //the number of bytes of the body we already received.
                while (alreadyhave != contentlen)
                {
                    int t = s.Read(buffer, 0, 1024);
                    if (t == 0) break;
                    alreadyhave += t;
                    ms.Write(buffer, 0, t);
                }

                m.Body = Encoding.UTF8.GetString(ms.ToArray(), i, (int)contentlen); //cast is ok because we checked bounds above.
            }

            return m;
        }

        /// <summary>
        /// Find needle in haystack, starting from index pos. Returns position of start of needle if found, -1 if not.
        /// </summary>
        /// <param name="needle"></param>
        /// <param name="haystack"></param>
        /// <param name="pos"></param>
        /// <returns></returns>
        public static int FindByteSequence(byte[] needle, byte[] haystack, int pos)
        {
            for (int i = pos; i <= haystack.Count() - pos; i++)
            {
                if (needle.SequenceEqual(haystack.Skip(i).Take(needle.Count())))
                    return i;
            }
            return -1;
        }

        public byte[] GetUnicodeBytes()
        {
            return Encoding.UTF8.GetBytes(this.Serialize());
        }
    }

    public enum HttpMessageType
    {
        REQUEST,
        RESPONSE
    };

    public sealed class HttpMethod
    {
        public static readonly HttpMethod GET = new HttpMethod("GET");
        public static readonly HttpMethod POST = new HttpMethod("POST");

        private string m;

        private HttpMethod(string m)
        {
            this.m = m;
        }

        public override string ToString()
        {
            return m;
        }
    }

    public sealed class HttpResponseCode
    {
        public int Code {get; private set; }
        private string message;

        public static readonly HttpResponseCode OK = new HttpResponseCode(200, "OK");
        public static readonly HttpResponseCode NOT_FOUND = new HttpResponseCode(404, "Not Found");
        public static readonly HttpResponseCode INT_SRV_ERR = new HttpResponseCode(500, "Internal Server Error");

        private HttpResponseCode(int code, string message)
        {
            this.Code = code;
            this.message = message;
        }

        public override string ToString()
        {
            return Code + " " + message;
        }
    }
}
