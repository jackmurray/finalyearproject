using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;

namespace SpeakerController
{
    [ServiceContract]
    interface IControllerService
    {
        [OperationContract]
        int GetVersion();

        [OperationContract]
        byte[] GetCurrentDataKey();
    }
}
