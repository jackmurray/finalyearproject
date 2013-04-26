using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using LibSSDP;

namespace Tester
{
    class Program
    {
        static void Main(string[] args)
        {
            new SSDPService(Guid.NewGuid()).Start();
        }
    }
}
