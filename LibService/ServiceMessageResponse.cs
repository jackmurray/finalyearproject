using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibService
{
    public class ServiceMessageResponse
    {
        /// <summary>
        /// The message payload.
        /// </summary>
        public string Data { get; set; }

        public HttpResponseCode ResponseCode { get; set; }

        public ServiceMessageResponse(string data, HttpResponseCode code)
        {
            this.Data = data;
            this.ResponseCode = code;
        }
    }
}
