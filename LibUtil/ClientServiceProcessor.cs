using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Text;
using System.ServiceModel;

namespace LibUtil
{
    public class ClientServiceProcessor : IClientMessageInspector, IEndpointBehavior
    {
        private IWebServiceMessageProcessor _processor;

        public ClientServiceProcessor(IWebServiceMessageProcessor processor)
        {
            _processor = processor;
        }

        public object BeforeSendRequest(ref Message request, IClientChannel channel)
        {
            _processor.ProtectMessage(ref request);
            return null;
        }

        public void AfterReceiveReply(ref Message reply, object correlationState)
        {
            if (!_processor.VerifyMessage(reply))
            {
                throw new FormatException("Reply failed verification");
            }
            return;
        }

        public void Validate(ServiceEndpoint endpoint)
        {
            
        }

        public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
        {
            
        }

        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
        {
            
        }

        public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
            clientRuntime.MessageInspectors.Add(this);
        }
    }
}
