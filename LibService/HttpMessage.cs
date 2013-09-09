using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibService
{
    public class HttpMessage
    {
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
            HttpMessage m = new HttpMessage();
            int endOfHeaders = s.IndexOf("\r\n\r\n");

            string[] headers = s.Substring(0, endOfHeaders).Split(new[] {"\r\n"}, StringSplitOptions.RemoveEmptyEntries);
            if (headers.Length < 1)
                throw new FormatException("No headers found.");
            string[] firstline = headers[0].Split(' ');
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

            string body = s.Substring(endOfHeaders + 4);
            m.Body = body;

            return m;
        }

        public byte[] GetUnicodeBytes()
        {
            return Encoding.Unicode.GetBytes(this.Serialize());
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
