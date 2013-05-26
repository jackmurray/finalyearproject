using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Text;
using System.ServiceModel;

namespace LibSecurity
{
    public class ServerRequestProcessor : IDispatchMessageInspector, IEndpointBehavior
    {
        private IWebServiceMessageProcessor _processor;

        public ServerRequestProcessor(IWebServiceMessageProcessor processor)
        {
            _processor = processor;
        }

        public object AfterReceiveRequest(ref Message request, IClientChannel channel, InstanceContext instanceContext)
        {
            if (!_processor.VerifyMessage(request))
                throw new FaultException("Incoming message failed verification"); //We can throw this here, and WCF will take the exception and deal with it.
            return null;
        }

        public void BeforeSendReply(ref Message reply, object correlationState)
        {
            reply = _processor.ProtectMessage(ref reply);
            //reply = Message.CreateMessage(MessageVersion.Soap11, "test");
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
            endpointDispatcher.DispatchRuntime.MessageInspectors.Add(this);
        }

        public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
            
        }
    }
}
