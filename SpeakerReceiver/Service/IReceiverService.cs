using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;

namespace SpeakerReceiver
{
    [ServiceContract]
    interface IReceiverService
    {
        [OperationContract]
        int GetVersion();
    }
}
