﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.18034
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace SpeakerController.ReceiverService {
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName="ReceiverService.IReceiverService")]
    public interface IReceiverService {
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IReceiverService/GetVersion", ReplyAction="http://tempuri.org/IReceiverService/GetVersionResponse")]
        int GetVersion();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IReceiverService/GetVersion", ReplyAction="http://tempuri.org/IReceiverService/GetVersionResponse")]
        System.Threading.Tasks.Task<int> GetVersionAsync();
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface IReceiverServiceChannel : SpeakerController.ReceiverService.IReceiverService, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class ReceiverServiceClient : System.ServiceModel.ClientBase<SpeakerController.ReceiverService.IReceiverService>, SpeakerController.ReceiverService.IReceiverService {
        
        public ReceiverServiceClient() {
        }
        
        public ReceiverServiceClient(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public ReceiverServiceClient(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public ReceiverServiceClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public ReceiverServiceClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        public int GetVersion() {
            return base.Channel.GetVersion();
        }
        
        public System.Threading.Tasks.Task<int> GetVersionAsync() {
            return base.Channel.GetVersionAsync();
        }
    }
}
