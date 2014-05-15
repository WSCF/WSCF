using System;
using System.Collections.Generic;
using System.Text;
using System.ServiceModel.Description;
using System.ServiceModel.Channels;

namespace Thinktecture.Tools.Web.Services.ServiceDescription
{
    internal class ServiceEndpointFactory<T> where T:class
    {
        private static ContractDescription contractDescription = ContractDescription.GetContract(typeof(T));

        public static ServiceEndpoint CreateServiceEndpoint(Binding binding)
        {
            ServiceEndpoint ep = new ServiceEndpoint(contractDescription);
            ep.Binding = binding;
            return ep;
        }
    }
}
